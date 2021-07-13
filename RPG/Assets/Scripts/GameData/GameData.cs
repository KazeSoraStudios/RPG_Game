using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;
using RPG_GameState;

namespace RPG_GameData
{
    public sealed class GameData : MonoBehaviour
    {
        public DictionaryList<string, ItemInfo> Items = new DictionaryList<string, ItemInfo>();
        public Dictionary<string, ItemUse> ItemUses = new Dictionary<string, ItemUse>();
        public Dictionary<string, PartyMemeberDefintion> PartyDefs = new Dictionary<string, PartyMemeberDefintion>();
        public Dictionary<string, Dictionary<Stat, int>> Stats = new Dictionary<string, Dictionary<Stat, int>>();
        public Dictionary<string, Spell> Spells = new Dictionary<string, Spell>();
        public Dictionary<string, Spell> Specials = new Dictionary<string, Spell>();
        public Dictionary<string, Enemy> Enemies = new Dictionary<string, Enemy>();
        public Dictionary<string, Quest> Quests = new Dictionary<string, Quest>();
        public Dictionary<string, Area> Areas = new Dictionary<string, Area>();
        public Dictionary<string, Shop> Shops = new Dictionary<string, Shop>();
        public Dictionary<string, Encounter> Encounters = new Dictionary<string, Encounter>();

        void Awake()
        {
            ServiceManager.Register(this);
        }

        void OnDestroy()
        {
            LogManager.LogWarn("GameData shutting down. Is game finished?");
            ServiceManager.Unregister(this);
        }

        public Dictionary<Stat, int> GetStatsForActor(string actor)
        {
            return new Dictionary<Stat, int>();
        }
    }

    public class PartyMemeberDefintion
    {
        public int Level;
        public string Id;
        public string Name;
        public string Portrait;
        public string StatsId;
        public StatGrowth StatGrowth;
        public ActionGrowth ActionGrowth;

        public string LocalizedName()
        {
            return ServiceManager.Get<LocalizationManager>().Localize(Name);
        }
    }

    public class ActionGrowth
    {
        public Dictionary<int, List<string>> Spells = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> Special = new Dictionary<int, List<string>>();
    }

    public enum SpellElement { Fire, Ice, Electric, Heal, Special, None }

    public class Spell
    {
        public string Id;
        public string Name;
        public string Description;
        public string Action;
        public SpellElement SpellElement;
        public int MpCost;
        public Vector2 BaseDamage;
        public float HitChance;
        public ItemTarget ItemTarget;

        public string LocalizedName()
        {
            return ServiceManager.Get<LocalizationManager>().Localize(Name);
        }
    }

    public class Enemy
    {
        public int StealItem;
        public int Exp;
        public Vector2 Gold;
        public string Id;
        public string Name;
        public string StatsId;
        public string Portrait;
        public string PrefabPath;
        public List<string> Spells = new List<string>();
        public List<string> Specials = new List<string>();
        public List<Vector3> ItemDrops = new List<Vector3>();

        public string GetName()
        {
            return ServiceManager.Get<LocalizationManager>().Localize(Name);
        }
    }

    public class Quest
    {
        public bool IsStoryQuest;
        public bool IsStarted;
        public bool IsComplete;
        public int Gold;
        public int Exp;
        public string Id;
        public Dictionary<string, int> Rewards;
        public LootData Loot;
        public QuestCondition Condition;

        public string LocalizedNamed()
        {
            var name = string.Format(Constants.ID_QUEST_NAME_FORMAT, Id);
            return ServiceManager.Get<LocalizationManager>().Localize(name);
        }

        public string LocalizedDescription()
        {
            var description = string.Format(Constants.ID_QUEST_DESCRIPTION_FORMAT, Id);
            return ServiceManager.Get<LocalizationManager>().Localize(description);
        }

        public LootData CreateReward()
        {
            var items = ServiceManager.Get<GameData>().Items;
            var rewards = new List<Item>();
            foreach (var reward in Rewards)
            {
                if (!items.Contains(reward.Key))
                {
                    LogManager.LogError($"Reward [{reward.Key}] not found in GameData Items.");
                    continue;
                }
                var item = items[reward.Key];
                rewards.Add(new Item { ItemInfo = item, Count = reward.Value });
            }
            return new LootData
            {
                Gold = Gold,
                Exp = Exp,
                Loot = rewards
            };
        }

        public bool TryToComplete()
        {
            if (IsComplete)
                return false;
            Condition.TryToComplete();
            IsComplete = Condition.IsComplete();
            return IsComplete;
        }

        public void LoadFromQuestData(QuestData data)
        {
            IsStarted = data.IsStarted;
            IsComplete = data.IsComplete;
        }
    }

    [Serializable]
    public class Area
    {
        public string Id;
        public new Dictionary<string, bool> Events = new Dictionary<string, bool>();
        public new Dictionary<string, bool> Chests = new Dictionary<string, bool>();
        public new Dictionary<string, bool> Items = new Dictionary<string, bool>();
    }

    public class Shop
    {
        public string Id;
        public new List<string> Items = new List<string>();
        public new Dictionary<string, List<string>> AdditionalItems = new Dictionary<string, List<string>>();

        public List<string> GetAllAvilableItems()
        {
            var items = new List<string>();
            items.AddRange(Items);
            var gameState = ServiceManager.Get<GameStateManager>().GetCurrent();
            if (gameState == null)
                return items;
            foreach (var entry in AdditionalItems)
            {
                var conditionInfo = entry.Key.Split(':');
                if (conditionInfo.Length != 2)
                    continue;
                var area = gameState.GetArea(conditionInfo[0]);
                if (area == null)
                    continue;
                if (!area.Events.ContainsKey(conditionInfo[1]) || !area.Events[conditionInfo[1]])
                    continue;
                items.AddRange(entry.Value);   
            }
            return items;
        }
    }

    public class Encounter
    {
        public string Id;
        public List<string> Backgrounds = new List<string>();
        public List<OddmentTable> Encounters = new List<OddmentTable>();
        
        public string GetBackground(int encounter)
        {
            if (Backgrounds.Count < 1)
                return Constants.DEFAULT_COMBAT_BACKGROUND;
            if (encounter < 0 || encounter > Backgrounds.Count)
                return Backgrounds[0];
            return Backgrounds[encounter];
        }

        public OddmentTable GetEncounter(int encounter)
        {
            if (encounter < 0 || encounter > Encounters.Count)
                return new OddmentTable();
            return Encounters[encounter];
        }
    }
}