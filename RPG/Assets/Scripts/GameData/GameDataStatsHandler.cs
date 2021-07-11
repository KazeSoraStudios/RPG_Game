using System.Collections.Generic;
using RPG_Character;

namespace RPG_GameData
{
    public class GameDataStatsHandler : GameDataHandler
    {
        public static Dictionary<string, Dictionary<Stat, int>> ProcessStats(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Stats.");
            LogManager.LogDebug($"Processing Stats for data {data}");
            var statDefitions = new Dictionary<string, Dictionary<Stat, int>>();
            for (int i = 0; i < count; i++)
            {
                var id = data[index++];
                var stats = new Dictionary<Stat, int>
                {
                    { Stat.HP, int.Parse(data[index++])},
                    { Stat.MP, int.Parse(data[index++])},
                    { Stat.Strength, int.Parse(data[index++])},
                    { Stat.Defense, int.Parse(data[index++])},
                    { Stat.Magic, int.Parse(data[index++])},
                    { Stat.Resist, int.Parse(data[index++])},
                    { Stat.Speed, int.Parse(data[index++])}
                };
                index += columnAdvance + 3;
                statDefitions.Add(id, stats);
            }
            LogManager.LogDebug("Processing Gamedata Stats finished.");
            return statDefitions;
        }
    }
}