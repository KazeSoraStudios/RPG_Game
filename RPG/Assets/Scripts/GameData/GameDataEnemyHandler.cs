using System.Collections.Generic;
using UnityEngine;

public class GameDataEnemyHandler : GameDataHandler
{
    public static Dictionary<string, Enemy> PrcoessEnemies(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData Enemies.");
        LogManager.LogDebug($"Processing Enemies for data {data}");
        var enemies = new Dictionary<string, Enemy>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 8;
        for (int i = 0; i < count; i++)
        {
            var enemy = new Enemy()
            {
                Id = data[index++],
                Stats = GetStats(data[index++]),
                Name = data[index++],
                Portrait = data[index++],
                StealItem = GetIntFromCell(data[index++]),
                Spells = GetSpells(data[index++]),
                Specials = GetSpells(data[index++]),
                Exp = GetIntFromCell(data[index++]),
                Gold = GetVector2FromCell(data[index++]),
                ItemDrops = GetItemDrops(data[index++])
            };
            index += columnDifference;
            enemies.Add(enemy.Id, enemy);
        }
        LogManager.LogDebug("Processing Gamedata Enemies finished.");
        return enemies;
    }

    private static Dictionary<RPG_Character.Stat, int> GetStats(string data)
    {
        var stats = new Dictionary<RPG_Character.Stat, int>();
        if (data.IsEmpty())
        {
            LogManager.LogError($"Invalid data passed to GetStats in GameDataEnemyHandler.");
            return stats;
        }
        var statData = data.Split(';');
        foreach(var value in statData)
        {
            var info = value.Split(':');
            if (info.Length != 2)
            {
                
                LogManager.LogError($"Info [{string.Join(", ", info)}] is in invalid format in GetStats in GameDataEnemyHandler.");
                continue;
            }
            var stat = GetEnum(RPG_Character.Stat.HP, info[0]);
            var amount = GetIntFromCell(info[1]);
            stats.Add(stat, amount);
        }
        return stats;
    }

    private static List<string> GetSpells(string data)
    {
        var spells = new List<string>();
        var spellIds = data.Split(';');
        foreach(var id in spellIds)
            if (!id.IsEmptyOrWhiteSpace())
                spells.Add(id);
        return spells;
    }

    private static List<Vector3> GetItemDrops(string data)
    {
        var drops = new List<Vector3>();
        var items = data.Split(';');
        foreach(var item in items)
            drops.Add(GetVector3FromCell(item));
        return drops;
    }
}
