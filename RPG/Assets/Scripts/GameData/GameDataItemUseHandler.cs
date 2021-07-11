using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Combat;

namespace RPG_GameData
{
    public class GameDataItemUseHandler : GameDataHandler
    {
        public static Dictionary<string, ItemUse> ProcessItems(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData ItemUses.");
            LogManager.LogDebug($"Processing ItemUse for data {data}");
            var items = new Dictionary<string, ItemUse>();
            for (int i = 0; i < count; i++)
            {
                var name = data[index++];
                var itemUse = new ItemUse()
                {
                    Id = name,
                    Amount = GetIntFromCell(data[index++]),
                    Hint = data[index++],
                    Action = data[index++],
                    UseOnMap = data[index++].Equals("1"),
                    Target = new ItemTarget
                    {
                        Selector = GetTarget(name, data[index++]),
                        SwitchSides = data[index++].Equals("1"),
                        Type = GetEnum(CombatTargetType.One, data[index++])
                    }
                };
                index += columnAdvance;
                items.Add(name, itemUse);
            }
            LogManager.LogDebug("Processing Gamedata ItemUses finished.");
            return items;
        }
    }
}