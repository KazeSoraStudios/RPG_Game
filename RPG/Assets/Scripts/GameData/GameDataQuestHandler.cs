using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataQuestHandler : GameDataHandler
    {
        public static Dictionary<string, Quest> ProcessQuests(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Quests.");
            LogManager.LogDebug($"Processing quests for data {data}");
            var quests = new Dictionary<string, Quest>();
            for (int i = 0; i < count; i++)
            {
                var id = data[index++];
                var quest = new Quest()
                {
                    Id = id,
                    Gold = GetIntFromCell(data[index++]),
                    Exp = GetIntFromCell(data[index++]),
                    Rewards = GetRewards(data[index++]),
                    Condition = BuildCondition(data[index++]),
                    IsStoryQuest = data[index++].Equals("1")
                };
                index += columnAdvance;
                quests.Add(id, quest);
            }
            LogManager.LogDebug("Processing Gamedata Quests finished.");
            return quests;
        }

        private static Dictionary<string, int> GetRewards(string rewardData)
        {
            var rewards = new Dictionary<string, int>();
            if (rewardData.IsEmptyOrWhiteSpace())
                return rewards;
            var items = rewardData.Split(',');
            foreach (var item in items)
            {
                var itemInfo =  item.Split(';');
                if (itemInfo.Length != 2)
                {
                    LogManager.LogError($"Incorrect format for reward [{itemInfo}] in GetRewards.");
                    continue;
                }
                if (!int.TryParse(itemInfo[1], out int count))
                {
                    LogManager.LogError($"Incorrect format for reward amount [{itemInfo[1]}] in GetRewards.");
                    continue;
                }
                rewards.Add(itemInfo[0], count);
            }
            return rewards;
        }

        private static QuestCondition BuildCondition(string conditionData)
        {
            string itemCondition = "item";
            var requirements = new List<QuestRequirement>();
            if (conditionData.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError($"Condition Data [{conditionData}] is in incorrect format for BuildCondition.");
                return new QuestCondition(requirements);
            }
            var conditions = conditionData.Split(',');
            foreach (var condition in conditions)
            {
                var info = condition.Split(';');
                if (info.Length != 2)
                {
                    LogManager.LogError($"Condition [{info}] is in incorrect format for BuildCondition.");
                    continue;
                }
                var isSpecial = false;
                int countIndex = 1;
                int itemIndex = 0;
                if (info[0].Equals(itemCondition))
                {
                    var items = info[1].Split(':');
                    if (items.Length < 2)
                    {
                        LogManager.LogError($"Condition [{items}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    else if (items.Length == 3)
                    {
                        isSpecial = true;
                        countIndex = 2;
                        itemIndex = 1;
                    }
                    else if (items.Length > 3)
                    {
                        LogManager.LogError($"Condition item info [{items}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    if (!int.TryParse(items[countIndex], out var count))
                    {
                        LogManager.LogError($"Condition item info count [{items}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    if (isSpecial)
                        requirements.Add(new SpecialItemQuestRequirement(items[itemIndex], count));
                    else
                        requirements.Add(new ItemQuestRequirement(items[itemIndex], count));
                }
                else
                {
                    var enemies = info[1].Split(';');
                    if (enemies.Length < 2)
                    {
                        LogManager.LogError($"Condition [{enemies}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    else if (info.Length == 3)
                    {
                        isSpecial = true;
                        countIndex = 2;
                        itemIndex = 1;
                    }
                    else if (info.Length > 3)
                    {
                        LogManager.LogError($"Condition item info [{info}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    if (!int.TryParse(info[countIndex], out var count))
                    {
                        LogManager.LogError($"Condition item info count [{info}] is in incorrect format for BuildCondition.");
                        continue;
                    }
                    requirements.Add(new EnemyItemRequirement(enemies[itemIndex], count, isSpecial));
                }
            }
            return new QuestCondition(requirements);
        }
    }
}