﻿using System;
using System.Collections.Generic;
using RPG_GameData;

namespace RPG_GameState
{
    [Serializable]
    public class GameStateData
    {
        public class Config
        {
            public int Gold;
            public float PlayTime;
            public List<CharacterInfo> PartyMembers = new List<CharacterInfo>();
            public List<ItemData> Items = new List<ItemData>();
            public List<ItemData> KeyItems = new List<ItemData>();
            public List<QuestData> Quests = new List<QuestData>();
        }

        private static int gid = 0;
        private int id;

        public int gold;
        public float playTime;
        public List<CharacterInfo> partyMembers = new List<CharacterInfo>();
        public List<ItemData> items = new List<ItemData>();
        public List<ItemData> keyItems = new List<ItemData>();
        public List<QuestData> quests = new List<QuestData>();

        public GameStateData() { }

        public GameStateData(Config config)
        {
            SetData(config);
        }

        public void SetData(Config config)
        {
            if (config == null)
            {
                LogManager.LogError("Null Config passed to GameState SetData.");
                return;
            }
            gold = config.Gold;
            playTime = config.PlayTime;
            partyMembers = config.PartyMembers;
            items = config.Items;
            quests = config.Quests;
        }

        public QuestData GetQuestData(string id)
        {
            foreach (var quest in quests)
                if (quest.Id.Equals(id))
                    return quest;
            return null;
        }
    }

    [Serializable]
    public class ItemData
    {
        public int Count;
        public string Id;

        public static List<ItemData> FromItems(List<Item> items)
        {
            var itemData = new List<ItemData>();
            foreach (var quest in items)
                itemData.Add(FromItem(quest));
            return itemData;
        }

        public static ItemData FromItem(Item item)
        {
            return new ItemData { Count = item.Count, Id = item.ItemInfo.Id};
        }
    }

    [Serializable]
    public class QuestData
    {
        public bool IsStarted;
        public bool IsComplete;
        public string Id;

        public static List<QuestData> FromQuests(List<Quest> quests)
        {
            var questData = new List<QuestData>();
            foreach (var quest in quests)
                questData.Add(FromQuest(quest));
            return questData;
        }

        public static QuestData FromQuest(Quest quest)
        {
            return new QuestData { Id = quest.Id, IsComplete = quest.IsComplete, IsStarted = quest.IsStarted };
        }
    }
}