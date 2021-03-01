using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataLocalizationHandler : GameDataHandler
{
    public static Dictionary<string, string> ProcessLocaliation(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData Localization.");
        LogManager.LogDebug($"Processing Localization for data {data}");
        var terms = new Dictionary<string, string>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 2;
        for (int i = 0; i < count; i++)
        {
            var key = data[index++];
            var value = data[index++];
            terms.Add(key, value);
            index += columnDifference;
        }
        LogManager.LogDebug("Processing Gamedata Localization finished.");
        return terms;
    }
}
