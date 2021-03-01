using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataStatsHandler : GameDataHandler
{
    public static Dictionary<string, Dictionary<Stat, int>> ProcessStats(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData Stats.");
        LogManager.LogDebug($"Processing Stats for data {data}");
        var statDefitions = new Dictionary<string, Dictionary<Stat, int>>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 6;
        for (int i = 0; i < count; i++)
        {
            var id = data[index++];
            var stats = new Dictionary<Stat, int>
            {
                { Stat.HP, int.Parse(data[index++])},
                { Stat.MP, int.Parse(data[index++])},
                { Stat.Strength, int.Parse(data[index++])},
                { Stat.Speed, int.Parse(data[index++])},
                { Stat.Intelligence, int.Parse(data[index++])}
            };
            index += columnDifference;
            statDefitions.Add(id, stats);
        }
        LogManager.LogDebug("Processing Gamedata Stats finished.");
        return statDefitions;
    }
}
