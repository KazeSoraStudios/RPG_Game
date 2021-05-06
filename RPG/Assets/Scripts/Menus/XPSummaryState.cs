using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;
using TMPro;

namespace RPG_UI
{
    public class XPSummaryState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public StateStack Stack;
            public List<Actor> Party;
            public LootData Loot;
        }

        [SerializeField] TextMeshProUGUI ExpText;
        [SerializeField] ActorXPSummary[] XPSummaries;

        private bool isCountingXp;
        private float xp;
        private float xpCounter = 0;
        private float xpPerSecond = 5.0f;
        private Config config;
        private List<XPPopUp> pool = new List<XPPopUp>();

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            this.config = config;
            xp = config.Loot.Exp;
            // Award more exp faster
            var digit = Mathf.Log10(xp + 1);
            xpPerSecond *= digit * digit;

            int i = 0;
            for (; i < config.Party.Count && i < XPSummaries.Length; i++)
                XPSummaries[i].Init(config.Party[i], this);
            for (; i < XPSummaries.Length; i++)
                XPSummaries[i].gameObject.SafeSetActive(false);
        }

        public void Enter(object o = null)
        {
            isCountingXp = true;
            xpCounter = 0;
            gameObject.SafeSetActive(true);
        }

        public bool Execute(float deltaTime)
        {
            foreach (var summary in XPSummaries)
                if (summary.gameObject.activeSelf)
                    summary.Execute(deltaTime);
            if (!isCountingXp)
                return true;
            xpCounter += xpPerSecond * deltaTime;
            var xpToApply = Mathf.Floor(xpCounter);
            xp -= xpToApply;
            ApplyXPToParty((int)xpToApply);
            if (xp <= 0)
                isCountingXp = false;
            return true;
        }

        public void Exit() { }

        public string GetName()
        {
            return "XPSummaryState";
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (xp > 0)
                {
                    SkipCountingXP();
                    return;
                }
                if (ArePopUpsRemaining())
                {
                    CloseNextPopUp();
                    return;
                }
                GotoLootSummary();
            }
        }

        public bool HasPopUpInPool()
        {
            return pool.Count > 0;
        }

        public XPPopUp GetPopUpFromPool()
        {
            if (pool.Count < 1)
            {
                LogManager.LogError("Tried to get PopuUp but pool is empty.");
                return null;
            }
            int index = pool.Count - 1;
            var cell = pool[index];
            pool.RemoveAt(index);
            return cell;
        }

        public void ReturnPopUpToPool(XPPopUp cell)
        {
            pool.Add(cell);
            cell.gameObject.SafeSetActive(false);
            cell.transform.SetParent(transform, false);
        }

        private void UnlockPopUps(ActorXPSummary summary, Actor.LevelUp levelup)
        { 
            foreach (var spell in levelup.Spells)
            {
                summary.AddPopUp(spell.LocalizedName(), Color.red);
            }

            foreach (var special in levelup.Specials)
            {
                summary.AddPopUp(special.LocalizedName(), Color.green);
            }
        }

        private void SkipCountingXP()
        {
            isCountingXp = false;
            xpCounter = 0;
            var xpToApply = Mathf.FloorToInt(xp);
            xp = 0;
            ApplyXPToParty(xpToApply);
        }

        private void GotoLootSummary()
        {
            var lootSummaryState = ServiceManager.Get<AssetManager>().Load</*LootSummaryState*/Object>(Constants.LOOT_SUMMARY_PREFAB);
            //LootSummaryState:Create(self.mStack,
            //gGame.World,
            //self.mCombatData)
            var events = new List<IStoryboardEvent>
            {
                StoryboardEventFunctions.BlackScreen("black", 1.0f),
                StoryboardEventFunctions.FadeScreenIn("black", 0.2f),
                StoryboardEventFunctions.ReplaceState(this, null/* lootSummaryState*/),
                StoryboardEventFunctions.Wait(0.1f),
                StoryboardEventFunctions.FadeScreenOut("black", 0.2f)
            };
            var storyboard = new Storyboard(config.Stack, events);
            config.Stack.Push(storyboard);
        }

        private void CloseNextPopUp()
        {
            foreach (var summary in XPSummaries)
                if (summary.gameObject.activeSelf) 
                    summary.CancelPopUp();
        }

        private void ApplyXPToParty(int xp)
        {
            for (int i = 0; i < config.Party.Count; i++)
            {
                var actor = config.Party[i];
                if (actor.Stats.Get(Stat.HP) <= 0)
                    continue;
                actor.AddExp(xp);
                while (actor.ReadyToLevelUp())
                {
                    var levelup = actor.CreateLevelUp();
                    var levelNumber = actor.Level + levelup.Level;
                    var summary = XPSummaries[i];
                    summary.AddPopUp("ID_LEVEL_UP_TEXT", Color.yellow);
                    UnlockPopUps(summary, levelup);
                    actor.ApplyLevel(levelup);

                }
            }
        }

        private bool ArePopUpsRemaining()
        {
            foreach (var summary in XPSummaries)
                if (summary.HasPopUps())
                    return true;
            return false;
        }
    }
}