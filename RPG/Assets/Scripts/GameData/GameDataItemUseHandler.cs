using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataItemUseHandler : GameDataHandler
{
    public static Dictionary<string, ItemUse> ProcessItems(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData ItemUses.");
        LogManager.LogDebug($"Processing ItemUse for data {data}");
        var items = new Dictionary<string, ItemUse>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 7;
        for (int i = 0; i < count; i++)
        {
            var name = data[index++];
            var itemUse = new ItemUse()
            {
                Id = name,
                Amount = GetIntFromCell(data[index++]),
                Hint = data[index++],
                UseOnMap = data[index++].Equals("1"),
                Target = new ItemTarget
                {
                    Selector = GetTarget(name, data[index++]),
                    SwitchSides = data[index++].Equals("1"),
                    Type = data[index++]
                }
            };
            index += columnDifference;
            items.Add(name, itemUse);
        }
        LogManager.LogDebug("Processing Gamedata ItemUses finished.");
        return items;
    }
}
