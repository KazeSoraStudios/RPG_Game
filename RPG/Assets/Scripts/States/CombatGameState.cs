using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG_Character;
using RPG_UI;
using TMPro;

namespace RPG_Combat
{
    public class LootData
    {
        public int Exp;
        public int Gold;
        public List<Item> Loot = new List<Item>();
    }

    public class CombatGameState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public bool CanFlee;
            public string BackgroundPath;
            public List<Actor> Party;
            public List<Actor> Enemies;
            public StateStack Stack;
            public Action OnWin;
            public Action OnDie;
        }

        [SerializeField] CombatMenuWidget CombatMenu;
        [SerializeField] CombatPositions Positions;
        [SerializeField] GameObject TipContainer;
        [SerializeField] TextMeshProUGUI TipText;
        [SerializeField] GameObject NoticeContainer;
        [SerializeField] TextMeshProUGUI NoticeText;
        [SerializeField] Image Background;

        public CombatBrowseListState ActionBrowseList;
        public CombatTargetState TargetState;
        public CombatChoiceState CombatChoice;
        public EventQueue EventQueue;
        public Actor SelectedActor;
        public StateStack CombatStack = new StateStack();
        public List<Actor> PartyActors = new List<Actor>();
        public List<Actor> EnemyActors = new List<Actor>();
        public List<Character> DeadCharacters = new List<Character>();
        public List<Character> PartyCharacters = new List<Character>();
        public List<Character> EnemyCharacters = new List<Character>();
        public Dictionary<int, Character> ActorToCharacterMap = new Dictionary<int, Character>();
        public List<object> Effects = new List<object>();
        public List<Drop> Drops = new List<Drop>();

        private bool canEscape = true;
        private bool escaped = false;
        private StateStack gameStack = new StateStack();
        private EventQueue eventQueue = new EventQueue();
        private Action onWin;
        private Action onDie;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(0, GetName()))
                return;
            EventQueue.Clear();
            CombatStack.Clear();
            canEscape = config.CanFlee;
            gameStack = config.Stack;
            PartyActors = config.Party;
            EnemyActors = config.Enemies;
            onWin = config.OnWin;
            onDie = config.OnDie;
            CreateCombatCharacters(true);
            CreateCombatCharacters(false);
            LoadMenuUI(config.Party);
            if (config.BackgroundPath.IsEmpty())
                config.BackgroundPath = "Textures/combat_bg_forest";
            //Background.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(config.BackgroundPath, () => Background.gameObject.SafeSetActive(true));
            PlaceActors();
            AddTurns(PartyActors, true);
            AddTurns(EnemyActors);
            RegisterEnemies();
        }

        public virtual bool Execute(float deltaTime)
        {
            foreach (var character in PartyCharacters)
                character.Controller.Update(deltaTime);
            foreach (var character in EnemyCharacters)
                character.Controller.Update(deltaTime);
            for (int i = DeadCharacters.Count - 1; i > -1; i--)
            {
                DeadCharacters[i].Controller.Update(deltaTime);
                var state = (CombatState)DeadCharacters[i].Controller.CurrentState;
                if (state == null || state.IsFinished())
                    DeadCharacters.RemoveAt(i);
            }

            // TODO effect updates

            if (CombatStack.Top() != StateStack.emptyState)
                CombatStack.Update(deltaTime);
            else
            {
                EventQueue.Execute();

                AddTurns(PartyActors);
                AddTurns(EnemyActors);

                if (PartyWins() || PartyFled())
                {
                    EventQueue.Clear();
                    OnWin();
                }
                else if (EnemyWins())
                {
                    EventQueue.Clear();
                    OnLose();
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
                foreach (var enemy in EnemyActors)
                    enemy.Stats.SetStat(Stat.HP, 0);

            return false;
        }

        public void HandleInput() { }
        public void Enter(object stateParams) { }
        public void Exit() 
        {
            var npcs = ServiceManager.Get<NPCManager>().ClearNpcsForMap(GetName());
            foreach (var npc in npcs)
                Destroy(npc.gameObject);
            Destroy(gameObject);
        }
        public List<Actor> GetPartyActors() { return PartyActors; }
        public List<Actor> GetEnemiesActors() { return EnemyActors; }
        public bool IsFinished() { return true; }
        public string GetName()
        {
            return "CombatGameState";
        }

        public List<Actor> GetAllActors()
        {
            var actors = new List<Actor>(PartyActors);
            actors.AddRange(EnemyActors);
            return actors;
        }

        public void ApplyCounter(Actor target, Actor attacker)
        {
            LogManager.LogDebug($"Target [{target.name}] countered Attacker [{attacker.name}].");
            var targetAlive = target.Stats.Get(Stat.HP) > 0;
            if (!targetAlive)
                return;
            var isPlayer = IsPartyMember(target);
            var config = new CEAttack.Config
            {
                IsCounter = true,
                IsPlayer = isPlayer,
                CombatState = this,
                Targets = new List<Actor>() { attacker },
                Actor = target
            };
            var attack = new CEAttack(config);
            int speed = -1;
            EventQueue.Add(attack, speed);

            //    // TODO add effect

            //self:AddSpriteEffect(target,
            //    gGame.Font.damageSprite['counter'])
        }

        public void ApplyMiss(Actor target)
        {
            LogManager.LogDebug($"Target [{target.name}] missed.");
            //AddSpriteEffect(target,
            //   gGame.Font.damageSprite['miss'])
        }

        public void ApplyDodge(Actor target)
        {
            LogManager.LogDebug($"Target [{target.name}] dodged.");
            if (target == null)
            {
                LogManager.LogError("Null target passed to Applydodge.");
                return;
            }
            if (!ActorToCharacterMap.TryGetValue(target.Id, out var character))
            {
                LogManager.LogError($"Actor {target} is not present in the ActorToCharacterMap.");
                return;
            }
            if (character.Controller.CurrentState.GetName() != Constants.HURT_STATE)
            {
                character.Controller.Change(Constants.HURT_STATE, new CombatStateParams { State = character.Controller.CurrentState.GetName() });
            }
            //AddSpriteEffect(target,
            //gGame.Font.damageSprite['dodge'])
        }

        public void ApplyDamage(Actor target, int damage, bool isCrit)
        {
            var stats = target.Stats;
            var hp = stats.Get(Stat.HP) - damage;
            stats.SetStat(Stat.HP, hp);
            LogManager.LogDebug($"{target.name} took {damage} damage and HP is now {hp}. Was critical hit: {isCrit}");
            if (!ActorToCharacterMap.TryGetValue(target.Id, out var character))
            {
                LogManager.LogError($"Actor {target} is not present in the ActorToCharacterMap.");
                return;
            }
            var controller = character.Controller;
            if (damage > 0 && controller.CurrentState.GetName() != Constants.HURT_STATE)
            {
                controller.Change(Constants.HURT_STATE, new CombatStateParams { State = controller.CurrentState.GetName() });
            }

            UpdateActorHp(target);
            /*
            local entity = character.mEntity
            local x = entity.mX
            local y = entity.mY

            local dmgColor = Vector.Create(1,1,1,1)

            if isCrit then
            dmgColor = Vector.Create(1,1,0,1)
            end

            local dmgEffect = JumpingNumbers:Create(x, y, damage, dmgColor)
            self:AddEffect(dmgEffect)
            end
            */

            //local dmgEffect = JumpingNumbers:Create(x, y, damage, dmgColor)
            //self: AddEffect(dmgEffect)
            HandleDeath();
        }

        public void UpdateActorHp(Actor actor)
        {
            var position = GetPartyPosition(actor);
            if (position != -1)
            {
                CombatMenu.UpdateHp(position, actor.Stats.Get(Stat.HP));
            }
        }

        public void UpdateActorMp(Actor actor)
        {
            var position = GetPartyPosition(actor);
            if (position != -1)
            {
                CombatMenu.UpdateMp(position, actor.Stats.Get(Stat.MP));
            }
        }

        public void HandleDeath()
        {
            HandlePartyDeath();
            HandleEnemyDeath();
        }

        public void ShowTip(string text)
        {
            TipContainer.SafeSetActive(true);
            TipText.SetText(text);
        }

        public void ShowNotice(string text)
        {
            NoticeContainer.SafeSetActive(true);
            NoticeText.SetText(text);
        }

        public void HideTip()
        {
            TipContainer.SafeSetActive(false);
        }

        public void HideNotice()
        {
            NoticeContainer.SafeSetActive(false);
        }

        public void OnFlee()
        {
            escaped = true;
        }

        private void PlaceActors()
        {
            if (Positions == null)
            {
                LogManager.LogError("Positions is null in CombatGameState.");
                return;
            }
            Positions.PlaceParty(PartyActors);
            Positions.PlaceEnemies(EnemyActors);
        }

        private void HandlePartyDeath()
        {
            foreach (var actor in PartyActors)
            {
                if (!ActorToCharacterMap.TryGetValue(actor.Id, out var character))
                {
                    LogManager.LogError($"Actor {actor} is not present in the ActorToCharacterMap.");
                    return;
                }
                if (!character.IsAnimationPlaying(Constants.DEATH_ANIMATION))
                {
                    var stats = actor.Stats;
                    var hp = stats.Get(Stat.HP);
                    if (hp <= 0)
                    {
                        character.Controller.Change(Constants.DIE_STATE, Constants.DEATH_ANIMATION);
                        EventQueue.RemoveEventsForActor(actor.Id);
                    }
                }
            }
        }

        private void HandleEnemyDeath()
        {
            for (int i = EnemyActors.Count - 1; i > -1; i--)
            {
                var actor = EnemyActors[i];
                var character = ActorToCharacterMap[actor.Id];
                var stats = actor.Stats;
                var hp = stats.Get(Stat.HP);
                if (hp <= 0)
                {
                    EnemyActors.RemoveAt(i);
                    EnemyCharacters.RemoveAt(i);
                    ActorToCharacterMap.Remove(actor.Id);
                    character.Controller.Change(Constants.ENEMY_DIE_STATE);
                    EventQueue.RemoveEventsForActor(actor.Id);
                    Drops.Add(actor.Loot);
                    DeadCharacters.Add(character);
                }
            }
        }

        private void OnWin()
        {
            if (gameStack.Top().GetHashCode() != GetHashCode())
                return;

            foreach (var actor in PartyActors)
            {
                var character = ActorToCharacterMap[actor.Id];
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive)
                    character.Controller.Change(Constants.DEATH_ANIMATION); // TODO victory animation
            }

            var combatData = CalculateCombatData();
            var xp = ServiceManager.Get<AssetManager>().Load<XPSummaryState>(Constants.XP_SUMMARY_MENU_PREFAB);
            var summary = Instantiate(xp);
            summary.gameObject.SafeSetActive(false);
            var layer = ServiceManager.Get<UIController>().MenuLayer;
            layer.gameObject.SafeSetActive(true);
            summary.transform.SetParent(layer, false);
            var config = new XPSummaryState.Config
            {
                Stack = gameStack,
                Party = ServiceManager.Get<World>().Party.Members,
                Loot = combatData
            };
            summary.Init(config);
            var events = new List<IStoryboardEvent>()
            {
                StoryboardEventFunctions.UpdateState(this, 1.0f),
                StoryboardEventFunctions.BlackScreen("black", 0),
                StoryboardEventFunctions.FadeScreenIn("black", 0.6f),
                StoryboardEventFunctions.ReplaceState(this, summary),
                StoryboardEventFunctions.Wait(0.3f),
                StoryboardEventFunctions.FadeScreenOut("black", 0.3f),
            };
            onWin?.Invoke();
            gameObject.SafeSetActive(false);
            
            FixCameraPosition();

            var storyboard = new Storyboard(gameStack, events);
            gameStack.Push(storyboard);

        }

        private void OnLose()
        {
            if (gameStack.Top().GetHashCode() == GetHashCode()) // TODO maybe fix?
                return;
            var events = new List<IStoryboardEvent>
            {
                StoryboardEventFunctions.UpdateState(this, 1.5f),
                StoryboardEventFunctions.BlackScreen(),
                StoryboardEventFunctions.FadeScreenIn("black"),
            };
            if (onDie != null)
            {
                events.Add(StoryboardEventFunctions.RemoveState(this));
                events.Add(StoryboardEventFunctions.Function(onDie));
            }
            else
            {
                var gameover = ServiceManager.Get<AssetManager>().Load<GameOverState>(Constants.GAME_OVER_PREFAB);
                var menu = Instantiate(gameover);
                var layer = ServiceManager.Get<UIController>().MenuLayer;
                menu.transform.SetParent(layer, false);
                var config = new GameOverState.Config
                {
                    Stack = CombatStack, // Stack,
                    World = ServiceManager.Get<World>()
                };
                menu.Init(config);
                events.Add(StoryboardEventFunctions.ReplaceState(this, menu));
            }
            events.Add(StoryboardEventFunctions.Wait(2.0f));
            events.Add(StoryboardEventFunctions.FadeScreenOut("black"));

            FixCameraPosition();

            var storyboard = new Storyboard(gameStack, events);
            gameStack.Push(storyboard);
        }

        private void CreateCombatCharacters(bool party)
        {
            List<Actor> actors;
            List<Character> characters;
            if (party)
            {
                actors = PartyActors;
                characters = PartyCharacters;
            }
            else
            {
                actors = EnemyActors;
                characters = EnemyCharacters;
            }

            foreach (var actor in actors)
            {
                // TODO check for special combat art or character
                var character = actor.GetComponent<Character>();
                characters.Add(character);
                ActorToCharacterMap[actor.Id] = character;
                character.Controller.Change(Constants.STAND_STATE);
            }
        }

        private void AddTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CETurn(actor, this);
                    var speed = forceFirst ? firstSpeed : turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding turn for {actor.name}");
                }
            }
        }

        private bool HasLiveActors(List<Actor> actors)
        {
            foreach (var actor in actors)
            {
                var stats = actor.Stats;
                if (stats.Get(Stat.HP) > 0)
                    return true;
            }
            return false;
        }

        private bool EnemyWins()
        {
            return !HasLiveActors(PartyActors);
        }

        private bool PartyWins()
        {
            return !HasLiveActors(EnemyActors);
        }

        private bool PartyFled()
        { 
            return escaped;
        }
        public bool IsPartyMember(Actor a)
        {
            foreach (var actor in PartyActors)
                if (actor.Id == a.Id)
                    return true;
            return false;
        }

        public int GetPartyPosition(Actor a)
        {
            for (int i = 0; i < PartyActors.Count; i++)
                if (PartyActors[i].Id == a.Id)
                    return i;
            return -1;
        }

        private LootData CalculateCombatData()
        {
            var data = new LootData();
            var lootDictionary = new Dictionary<string, Item>();
            foreach (var loot in Drops)
            {
                data.Exp += loot.Exp;
                data.Gold += loot.Gold;
                foreach (var item in loot.AlwaysDrop)
                {
                    if (!lootDictionary.ContainsKey(item.ItemInfo.Id))
                        lootDictionary[item.ItemInfo.Id] = item;
                    else
                        lootDictionary[item.ItemInfo.Id].Count += item.Count;
                }
                var chance = loot.PickChanceItem();
                if (chance == null)
                    continue;
                if (!lootDictionary.ContainsKey(chance.ItemInfo.Id))
                    lootDictionary[chance.ItemInfo.Id] = chance;
                else
                    lootDictionary[chance.ItemInfo.Id].Count += chance.Count;
            }
            return data;
        }

        private void LoadMenuUI(List<Actor> party)
        {
            var menuConfig = new CombatMenuWidget.Config
            {
                Actors = party
            };
            CombatMenu.Init(menuConfig);
        }

        private void RegisterEnemies()
        {
            var mapName = GetName();
            foreach (var enemy in EnemyCharacters)
                ServiceManager.Get<NPCManager>().AddNPC(mapName, enemy);
        }

        public void FixCameraPosition()
        {          
            UnityEngine.Object.Destroy(Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().m_Follow.gameObject);
            Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().m_Follow = PartyCharacters[0].transform;
        }
    }

    /*
function CombatState:OnPartyMemberSelect(index, data)

end
function CombatState:OnWin()
end

function CombatState:AddSpriteEffect(actor, sprite)
    local character = self.mActorCharMap[actor]
    local entity = character.mEntity
    local x = entity.mX
    local y = entity.mY
    local effect = CombatSpriteFx:Create(x, y, sprite)
    self:AddEffect(effect)
end

function CombatState:ApplyMiss(target)
    self:AddSpriteEffect(target,
        gGame.Font.damageSprite['miss'])
end
function CombatState:AddEffect(effect)

    for i = 1, #self.mEffectList do

        local priority = self.mEffectList[i].mPriority

        if effect.mPriority > priority then
            table.insert(self.mEffectList, i, effect)
            return
        end
    end

    table.insert(self.mEffectList, effect)
end
     
     */
}