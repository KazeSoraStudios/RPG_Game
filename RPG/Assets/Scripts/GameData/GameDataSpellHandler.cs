using System.Collections.Generic;

public class GameDataSpellHandler : GameDataHandler
{
    public static Dictionary<string, Spell> ProcessSpells(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData Spells.");
        LogManager.LogDebug($"Processing Spells for data {data}");
        var items = new Dictionary<string, Spell>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 11;
        for (int i = 0; i < count; i++)
        {
            var name = data[index++];
            var spell = new Spell()
            {
                Id = name,
                Name = data[index++],
                Action = data[index++],
                SpellElement = GetEnum(SpellElement.Heal, data[index++]),
                MpCost = GetIntFromCell(data[index++]),
                BaseDamage = GetVector2FromCell(data[index++]),
                HitChance = GetFloatFromCell(data[index++]),
                ItemTarget = new ItemTarget
                {
                    Selector = GetTarget(name, data[index++]),
                    SwitchSides = data[index++].Equals("1"),
                    Type = data[index++]
                },
                Description = data[index++]
            };
            index += columnDifference;
            items.Add(spell.Id, spell);
        }
        LogManager.LogDebug("Processing Gamedata Spells finished.");
        return items;
    }
}
