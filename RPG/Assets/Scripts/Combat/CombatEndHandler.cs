using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_UI;

namespace RPG_Combat
{
    public interface ICombatEndHandler
    {
        void Init(ICombatState state, Action onWin, Action onDie);
        void OnWin(StateStack stack);
        void OnLose(StateStack stack);
        List<Drop> Drops();
    }

    public class LootData
    {
        public int Exp;
        public int Gold;
        public List<Item> Loot = new List<Item>();
    }

    public class CombatEndHandler : ICombatEndHandler
    {
        private ICombatState state;
        private Action onWin;
        private Action onDie;
        private List<Drop> drops = new List<Drop>();

        public void Init(ICombatState state, Action onWin, Action onDie)
        {
            this.state = state;
            this.onWin = onWin;
            this.onDie = onDie;
        }

        public void OnWin(StateStack stack)
        {
            foreach (var actor in state.GetPartyActors())
            {
                var character = actor.GetComponent<Character>();
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive)
                    character.Controller.Change(Constants.DEATH_ANIMATION); // TODO victory animation
            }
            // Old flow,  XP -> Loot
            // var combatData = CalculateCombatData();
            // var xp = ServiceManager.Get<AssetManager>().Load<XPSummaryState>(Constants.XP_SUMMARY_MENU_PREFAB);
            // var summary = GameObject.Instantiate(xp);
            // summary.gameObject.SafeSetActive(false);
            // var layer = ServiceManager.Get<UIController>().MenuLayer;
            // layer.gameObject.SafeSetActive(true);
            // summary.transform.SetParent(layer, false);
            // var config = new XPSummaryState.Config
            // {
            //     Stack = stack,
            //     Party = ServiceManager.Get<World>().Party.Members,
            //     Loot = combatData
            // };
            // summary.Init(config);
            var events = new List<IStoryboardEvent>()
            {
                // Old flow,  XP -> Loot
                // StoryboardEventFunctions.UpdateState((IGameState)state, 1.0f),
                // StoryboardEventFunctions.BlackScreen("black", 0),
                // StoryboardEventFunctions.FadeScreenIn("black", 0.6f),
                // StoryboardEventFunctions.ReplaceState((IGameState)state, summary),
                // StoryboardEventFunctions.Wait(0.3f),
                // StoryboardEventFunctions.FadeScreenOut("black", 0.3f),
                StoryboardEventFunctions.BlackScreen(),
                StoryboardEventFunctions.Wait(1.0f),
                StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.3f),
                StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().LockInput()),
                StoryboardEventFunctions.Function(() => 
                {
                    ServiceManager.Get<Party>().ReturnFromCombat();
                    ServiceManager.Get<NPCManager>().ReturnFromCombat();
                    Actions.SetCameraToFollowHero();
                }),
                StoryboardEventFunctions.Function(() => onWin?.Invoke()),
                StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().UnlockInput())
            };
            var storyboard = new Storyboard(stack, events);
            stack.Pop();
            stack.Push(storyboard);
        }

        public void OnLose(StateStack stack)
        {
            var events = new List<IStoryboardEvent>
            {
                StoryboardEventFunctions.UpdateState((IGameState)state, 1.5f),
                StoryboardEventFunctions.BlackScreen(),
                StoryboardEventFunctions.FadeScreenIn("black"),
            };
            if (onDie != null)
            {
                events.Add(StoryboardEventFunctions.RemoveState((IGameState)state));
                events.Add(StoryboardEventFunctions.Function(onDie));
            }
            else
            {
                var gameover = ServiceManager.Get<AssetManager>().Load<GameOverState>(Constants.GAME_OVER_PREFAB);
                var menu = GameObject.Instantiate(gameover);
                var layer = ServiceManager.Get<UIController>().MenuLayer;
                menu.transform.SetParent(layer, false);
                var config = new GameOverState.Config
                {
                    Stack = state.Stack(),
                    World = ServiceManager.Get<World>()
                };
                menu.Init(config);
                events.Add(StoryboardEventFunctions.ReplaceState((IGameState)state, menu));
            }
            events.Add(StoryboardEventFunctions.Wait(2.0f));
            events.Add(StoryboardEventFunctions.FadeScreenOut("black"));
            var storyboard = new Storyboard(stack, events);
            stack.Push(storyboard);
        }        

        public List<Drop> Drops() => drops;

        private LootData CalculateCombatData()
        {
            var data = new LootData();
            var lootDictionary = new Dictionary<string, Item>();
            foreach (var loot in drops)
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
    }
}
