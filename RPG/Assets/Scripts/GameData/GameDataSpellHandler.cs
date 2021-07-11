using System.Collections.Generic;
using RPG_Combat;

namespace RPG_GameData
{
    public class GameDataSpellHandler : GameDataHandler
    {
        public static (Dictionary<string, Spell> spells, Dictionary<string, Spell> specials) ProcessSpells(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Spells.");
            LogManager.LogDebug($"Processing Spells for data {data}");
            var spells = new Dictionary<string, Spell>();
            var specials = new Dictionary<string, Spell>();
            for (int i = 0; i < count; i++)
            {
                var name = data[index++];
                var spell = new Spell()
                {
                    Id = name,
                    Name = data[index++],
                    Action = data[index++],
                    SpellElement = GetEnum(SpellElement.None, data[index++]),
                    MpCost = GetIntFromCell(data[index++]),
                    BaseDamage = GetVector2FromCell(data[index++]),
                    HitChance = GetFloatFromCell(data[index++]),
                    ItemTarget = new ItemTarget
                    {
                        Selector = GetTarget(name, data[index++]),
                        SwitchSides = data[index++].Equals("1"),
                        Type = GetEnum(CombatTargetType.One, data[index++])
                    },
                    Description = data[index++]
                };
                index += columnAdvance;
                if (spell.SpellElement == SpellElement.Special)
                    specials.Add(spell.Id, spell);
                else
                    spells.Add(spell.Id, spell);
            }
            LogManager.LogDebug("Processing Gamedata Spells finished.");
            return (spells, specials);
        }
    }
}