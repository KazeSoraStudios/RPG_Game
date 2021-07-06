using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;
using RPG_GameState;
namespace RPG_Character
{
    public enum Stat { HP, MaxHP, MP, MaxMP, Strength, Speed, Intelligence, Dodge, Counter, Attack, Defense, Resist, Magic };

    public class Stats
    {
        private Dictionary<Stat, int> stats = new Dictionary<Stat, int>();
        private StatModifier[] modifiers;

        private string name;
        private StatModifier EmptyModifier;

        public Stats(Dictionary<Stat, int> statDefinitions, string name)
        {
            this.name = name;
            SetUp();
            BuildStatsFromDefinition(statDefinitions);
        }

        public int GetTotalStats()
        {
            int total = 0;
            foreach (var entry in stats)
                total += Get(entry.Key);
            return total;
        }

        public bool HasStat(Stat stat)
        {
            return stats.ContainsKey(stat);
        }

        public void ResetHpMp()
        {
            if (stats.ContainsKey(Stat.HP) && stats.ContainsKey(Stat.MaxHP))
                stats[Stat.HP] = stats[Stat.MaxHP];
            if (stats.ContainsKey(Stat.MP) && stats.ContainsKey(Stat.MaxMP))
                stats[Stat.MP] = stats[Stat.MaxMP];
        }

        public int GetBaseStat(Stat stat)
        {
            return stats.ContainsKey(stat) ? stats[stat] : 0;
        }

        public void IncreaseStat(Stat stat, int value)
        {
            if (!stats.ContainsKey(stat))
            {
                LogManager.LogError($"Stats for Entity[{name}] does not contain stat [{stat}]. Cannot increase value");
                return;
            }
            stats[stat] += value;
        }

        public void SetStat(Stat stat, int value)
        {
            if (!stats.ContainsKey(stat))
            {
                LogManager.LogError($"Stats for Entity[{name}] does not contain stat [{stat}]. Cannot set value");
                return;
            }
            stats[stat] = Mathf.Max(0, value);
        }

        public void AddModifier(EquipSlot slot, StatModifier modifier)
        {
            if ((int)slot > modifiers.Length)
            {
                LogManager.LogError($"{slot} is greater than {name}'s modifiers. Not adding modifier[{modifier}]");
                return;
            }
            modifiers[(int)slot] = modifier;
        }

        public void RemoveModifier(EquipSlot slot)
        {
            if ((int)slot > modifiers.Length)
            {
                LogManager.LogError($"{slot} is greater than {name}'s modifiers. Not removing modifier");
                return;
            }
            modifiers[(int)slot] = EmptyModifier;
        }

        public int Get(Stat stat)
        {
            if (!stats.ContainsKey(stat))
                return 0;
            float multiplier = 0;
            int total = stats[stat];
            foreach (var modifier in modifiers)
            {
                total += modifier.GetAddValue(stat);
                multiplier += modifier.GetMultiplyValue(stat);
            }
            return (int)(total + (total * multiplier));
        }

        public float GetSpellElementModifer(SpellElement element)
        {
            float modifier = 0.0f;
            foreach(var m in modifiers)
                modifier += m.GetSpellElementValue(element);
            return modifier;
        }

        public int GetStatDiffForNewItem(Stat stat, EquipSlot slot, StatModifier newModifier)
        {
            if (newModifier == null)
                newModifier = EmptyModifier;
            if (!stats.ContainsKey(stat))
                return (int)(newModifier.GetAddValue(stat) + newModifier.GetMultiplyValue(stat));
            var currentValue = stats[stat];
            float multiplier = 0;
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (i == (int)slot)
                    continue;
                currentValue += modifiers[i].GetAddValue(stat);
                multiplier += modifiers[i].GetMultiplyValue(stat);
            }
            currentValue = (int)(currentValue + (currentValue * multiplier));
            var newValue = (int)(currentValue + newModifier.GetAddValue(stat) + (currentValue * (multiplier + newModifier.GetMultiplyValue(stat))));
            return currentValue + (newValue - currentValue);
        }

        public StatsData ToStatsData()
        {
            var statInfo = new List<StatInfo>();
            foreach (var stat in stats)
                statInfo.Add(new StatInfo { Stat = stat.Key, Value = stat.Value });
            return new StatsData { Stats = statInfo };
        }

        private void SetUp()
        {
            EmptyModifier = new StatModifier();
            var slots = (EquipSlot[])Enum.GetValues(typeof(EquipSlot));
            modifiers = new StatModifier[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                modifiers[i] = EmptyModifier;
        }

        private void BuildStatsFromDefinition(Dictionary<Stat, int> statDefinitions)
        {
            stats.Clear();
            foreach (var entry in statDefinitions)
                stats.Add(entry.Key, entry.Value);

            // MaxHP/MP are not included in the definition
            stats.Add(Stat.MaxHP, stats[Stat.HP]);
            stats.Add(Stat.MaxMP, stats[Stat.MP]);

            foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
                if (!stats.ContainsKey(stat))
                    stats[stat] = 0;
        }
    }

    public class StatModifier
    {
        public Dictionary<Stat, int> AddModifiers = new Dictionary<Stat, int>();
        public Dictionary<Stat, float> MultiplyModifiers = new Dictionary<Stat, float>();
        public Dictionary<SpellElement, float> SpellElementModifiers = new Dictionary<SpellElement, float>();

        public int GetAddValue(Stat stat)
        {
            return AddModifiers.ContainsKey(stat) ? AddModifiers[stat] : 0;
        }

        public float GetMultiplyValue(Stat stat)
        {
            return MultiplyModifiers.ContainsKey(stat) ? MultiplyModifiers[stat] : 0;
        }

        public float GetSpellElementValue(SpellElement element)
        {
            return SpellElementModifiers.ContainsKey(element) ? SpellElementModifiers[element] : 0;
        }
    }

    public class StatGrowth
    {
        public Dictionary<Stat, Dice> Growths = new Dictionary<Stat, Dice>();

        public bool HasGrowth(Stat stat)
        {
            return Growths.ContainsKey(stat);
        }
    }

    public class Dice
    {
        public List<Die> _Dice = new List<Die>();

        public void AddDie(Die die)
        {
            _Dice.Add(die);
        }

        public void RemoveDie(Die die)
        {
            for (int i = _Dice.Count - 1; i > -1; i--)
                if (_Dice[i].IsDie(die))
                {
                    _Dice.RemoveAt(i);
                    break;
                }
        }

        public int RollDice()
        {
            int total = 0;
            foreach (var die in _Dice)
                total += die.RollDie();
            return total;
        }
    }

    public struct Die
    {
        public int Rolls;
        public int Faces;
        public int Bonus;

        public int RollDie()
        {
            int total = 0;
            for (int i = 0; i < Rolls; i++)
                total = total + UnityEngine.Random.Range(1, Faces);
            return total + Bonus;
        }

        public bool IsDie(Die other)
        {
            return Rolls == other.Rolls &&
                Faces == other.Faces &&
                Bonus == other.Bonus;
        }
    }
}