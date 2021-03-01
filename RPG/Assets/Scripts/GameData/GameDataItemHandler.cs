using System;
using System.Collections.Generic;

public class GameDataItemHandler : GameDataHandler
{
    public static Dictionary<string, ItemInfo> ProcessItems(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData Items.");
        LogManager.LogDebug($"Processing items for data {data}");
        var items = new Dictionary<string, ItemInfo>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 8;
        for (int i = 0; i < count; i++)
        {
            var itemInfo = new ItemInfo()
            {
                Id = data[index++],
                Type = GetEnum(ItemType.None, data[index++]),
                Icon = data[index++],
                UseRestriction = GetEnumArray(UseRestriction.None, data[index++]),
                Description = data[index++],
                Stats = new ItemStats(data[index++]),
                Use = data[index++],
                Price = GetIntFromCell(data[index++])
            };
            index += columnDifference;
            items.Add(itemInfo.Id, itemInfo);
        }
        LogManager.LogDebug("Processing Gamedata Items finished.");
        return items;
    }
}
