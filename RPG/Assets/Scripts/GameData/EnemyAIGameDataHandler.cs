using System.Collections.Generic;
using RPG_AI;

namespace RPG_GameData
{
    public class EnemyAIGameDataHandler : GameDataHandler
    {
        public static Dictionary<string, EnemyAIData> PrcoessEnemyAI(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData EnemyAI.");
            LogManager.LogDebug($"Processing EnemyAI for data {data}");
            var aiData = new Dictionary<string, EnemyAIData>();
            for (int i = 0; i < count; i++)
            {
                var ai = new EnemyAIData()
                {
                    Id = data[index++],
                    HpThreshold = GetFloatFromCell(data[index++]),
                    MeleeLean = GetFloatFromCell(data[index++]),
                    Type = GetEnum<AIType>(AIType.Easy, data[index++])
                };
                index += columnAdvance;
                aiData.Add(ai.Id, ai);
            }
            LogManager.LogDebug("Processing Gamedata EnemyAI finished.");
            return aiData;
        }
    }
}
