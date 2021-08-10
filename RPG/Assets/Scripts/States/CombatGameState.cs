using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_AI;
using RPG_Character;
using RPG_UI;

namespace RPG_Combat
{
    public interface ICombatState
    {
        void OnFlee();
        bool IsSim();
        bool IsPartyMember(Actor actor);
        CombatUI GetUI();
        Node GetBehaviorTreeForAIType(AIType type);
        ICombatTurnHandler CombatTurnHandler();
        ICombatReporter CombatReporter();
        StateStack Stack();
        List<Actor> GetAllActors();
        List<Actor> GetEnemyActors();
        List<Actor> GetPartyActors();
    }

    public class CombatGameState : ConfigMonoBehaviour, IGameState, ICombatState
    {
        public class Config
        {
            public bool CanFlee;
            public bool Sim;
            public string BackgroundPath;
            public StateStack Stack;
            public Action OnWin;
            public Action OnDie;
            public ICombatReporter Reporter;
            public ICombatTurnHandler TurnHandler;
            public ICombatEndHandler EndHandler;
            public List<Actor> Party;
            public List<Actor> Enemies;
        }

        [SerializeField] CombatPositions Positions;
        [SerializeField] CombatUI CombatUI;

        public List<Actor> PartyActors = new List<Actor>();
        public List<Actor> EnemyActors = new List<Actor>();
        public List<Character> DeadCharacters = new List<Character>();
        public List<Character> PartyCharacters = new List<Character>();
        public List<Character> EnemyCharacters = new List<Character>();
        public Dictionary<AIType, Node> BehaviorTrees = new Dictionary<AIType, Node>();

        private bool sim = false;
        private bool canEscape = true;
        private bool escaped = false;
        private StateStack CombatStack = new StateStack();
        private StateStack gameStack = new StateStack();
        private ICombatReporter combatReporter;
        private ICombatTurnHandler turnHandler;
        private ICombatEndHandler endHandler;

        private void Awake() 
        {
            GameEventsManager.Register(GameEventConstants.ON_DAMAGE_TAKEN, HandleDamage);
        }

        private void OnDestroy() 
        {
            GameEventsManager.Unregister(GameEventConstants.ON_DAMAGE_TAKEN, HandleDamage);
        }

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(0, GetName()))
                return;
            CombatStack.Clear();
            canEscape = config.CanFlee;
            sim = config.Sim;
            gameStack = config.Stack;
            PartyActors = config.Party;
            EnemyActors = config.Enemies;
            CreateCombatCharacters(true);
            CreateCombatCharacters(false);
            LoadBehaviors();
            LoadInterfaces(config);
            CombatUI.LoadMenuUI(config.Party);
            var backgroundPath = config.BackgroundPath.IsEmpty() ? Constants.DEFAULT_COMBAT_BACKGROUND : config.BackgroundPath;
            ServiceManager.Get<CombatScene>()?.SetBackground(backgroundPath);
            PlaceActors();
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
                var state = DeadCharacters[i].Controller.CurrentState as CombatState;
                if (state == null || state.IsFinished())
                    DeadCharacters.RemoveAt(i);
            }

            // TODO effect updates

            if (CombatStack.Top() != StateStack.emptyState)
                CombatStack.Update(deltaTime);
            else
            {
                turnHandler.Execute();
                if (PartyWins() || PartyFled())
                {
                    turnHandler.ClearTurns();
                    OnWin();
                }
                else if (EnemyWins())
                {
                    turnHandler.ClearTurns();
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
            if (sim)
                return;
            var npcs = ServiceManager.Get<NPCManager>().ClearNpcsForMap(GetName());
            foreach (var npc in npcs)
                Destroy(npc.gameObject);
            Destroy(gameObject);
        }
        public List<Actor> GetPartyActors() { return PartyActors; }
        public List<Actor> GetEnemyActors() { return EnemyActors; }
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

        public void HandleDeath()
        {
            HandlePartyDeath();
            HandleEnemyDeath();
        }

        public void OnFlee()
        {
            escaped = true;
        }

        public Node GetBehaviorTreeForAIType(AIType type)
        {
            return BehaviorTrees.ContainsKey(type) ?
                BehaviorTrees[type] : null;
        }

        public bool IsSim() => sim;
        public StateStack Stack() => CombatStack;
        public ICombatTurnHandler CombatTurnHandler() => turnHandler;
        public CombatUI GetUI() => CombatUI;
        public ICombatReporter CombatReporter() => combatReporter;

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
                var character = actor.GetComponent<Character>();
                if (!character.IsAnimationPlaying(Constants.DEATH_ANIMATION))
                {
                    var stats = actor.Stats;
                    var hp = stats.Get(Stat.HP);
                    if (hp <= 0)
                    {
                        character.Controller.Change(Constants.DIE_STATE, Constants.DEATH_ANIMATION);
                        turnHandler.RemoveEventsForActor(actor.Id);
                    }
                }
            }
        }

        private void HandleEnemyDeath()
        {
            for (int i = EnemyActors.Count - 1; i > -1; i--)
            {
                var actor = EnemyActors[i];
                var character = actor.GetComponent<Character>();
                var stats = actor.Stats;
                var hp = stats.Get(Stat.HP);
                if (hp <= 0)
                {
                    EnemyActors.RemoveAt(i);
                    EnemyCharacters.RemoveAt(i);
                    character.Controller.Change(Constants.ENEMY_DIE_STATE);
                    turnHandler.RemoveEventsForActor(actor.Id);
                    endHandler.Drops().Add(actor.Loot);
                    DeadCharacters.Add(character);
                }
            }
        }

        private void OnWin()
        {
            if (gameStack.Top().GetHashCode() != GetHashCode())
                return;
            gameObject.SafeSetActive(false);
            endHandler.OnWin(gameStack);
        }

        private void OnLose()
        {
            if (gameStack.Top().GetHashCode() != GetHashCode()) // TODO maybe fix?
                return;
            endHandler.OnLose(gameStack);
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
                character.Controller.Change(Constants.STAND_STATE);
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
            return IsPartyMember(a.Id);
        }

        public bool IsPartyMember(int id)
        {
            foreach (var actor in PartyActors)
                if (actor.Id == id)
                    return true;
            return false;
        }

        private void RegisterEnemies()
        {
            if (sim) 
                return;
            var mapName = GetName();
            foreach (var enemy in EnemyCharacters)
                ServiceManager.Get<NPCManager>().AddNPC(mapName, enemy);
        }

        private void LoadBehaviors()
        {
            BehaviorTrees.Clear();
            BehaviorTrees = AIBehaviors.LoadEnemyBehaviors(this, EnemyActors);
        }

        private void HandleDamage(object obj)
        {
            if (obj == null || !(obj is Actor actor))
            {
                LogManager.LogError("Invalid object sent to HandleDamage.");
                return;
            }
            if (IsPartyMember(actor))
            {
                HandlePartyDeath();
                CombatUI.UpdateActorHp(GetPartyPosition(actor.Id), actor);
            }
            else
            {
                HandleEnemyDeath();
            }
        }

        private int GetPartyPosition(int id)
        {
            for (int i = 0; i < PartyActors.Count; i++)
                if (PartyActors[i].Id == id)
                    return i;
            return -1;
        }

        private void LoadInterfaces(Config config)
        {
            combatReporter = config.Reporter;
            if (combatReporter == null)
                combatReporter = new CombatInfoReporter(CombatUI);
            turnHandler = config.TurnHandler;
            if (turnHandler == null)
                turnHandler = new CombatTurnHandler();
            turnHandler.Init(this, GetComponent<EventQueue>());
            endHandler = config.EndHandler;
            if (endHandler == null)
                endHandler = new CombatEndHandler();
            endHandler.Init(this, config.OnWin, config.OnDie);
        }
    }
}