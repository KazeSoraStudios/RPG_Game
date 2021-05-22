using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;

namespace RPG_Character
{
    public enum EquipSlot { Weapon, Armor, Accessory1, Accessory2 };
    public class Actor : MonoBehaviour
    {
        private static int actorId = 0;
        public class LevelUp
        {
            public int Exp;
            public int Level;
            public Stats Stats;
            public List<Spell> Spells;
            public List<Spell> Specials;

            public LevelUp(int exp, int level, Stats stats)
            {
                Exp = exp;
                Level = level;
                Stats = stats;
                Spells = new List<Spell>();
                Specials = new List<Spell>();
            }
        }

        [SerializeField] public string Name;
        [SerializeField] public string Portrait;
        [SerializeField] public int Id;
        [SerializeField] public string PartyId;
        [SerializeField] public int Exp;
        [SerializeField] public int Level;
        [SerializeField] public int NextLevelExp;
        [SerializeField] public int StealItem;
        [SerializeField] public UseRestriction UseRestriction;
        [SerializeField] public List<Spell> Spells = new List<Spell>();
        [SerializeField] public List<Spell> Specials = new List<Spell>();
        //[SerializeField] MenuActions MenuActions SerializeObject
        [SerializeField] public Stats Stats;
        [SerializeField] public LevelFunction LevelFunction;
        [SerializeField] public StatGrowth StatGrowth = new StatGrowth();
        [SerializeField] public ItemInfo[] Equipment = new ItemInfo[3];
        [SerializeField] public Drop Loot = new Drop();

        public void Init(PartyMemeberDefintion partyMemeber)
        {
            if (partyMemeber == null)
            {
                LogManager.LogError($"Null party member passed to Actor[{name}]");
                return;
            }
            Spells.Clear();
            Specials.Clear();
            Id = actorId++;
            var gameData = ServiceManager.Get<GameData>();
            Stats = new Stats(gameData.Stats[partyMemeber.StatsId], name);
            StatGrowth = partyMemeber.StatGrowth;
            Portrait = partyMemeber.Portrait;
            Name = ServiceManager.Get<LocalizationManager>().Localize(partyMemeber.Name);
            Level = partyMemeber.Level;
            PartyId = partyMemeber.Id;
            var spells = partyMemeber.ActionGrowth.Spells;
            foreach (var spell in spells)
            {
                if (Level >= spell.Key)
                {
                    var list = GetSpellsForLevelUp(spell.Value);
                    Spells.AddRange(list);
                }
            }
            var specials = partyMemeber.ActionGrowth.Special;
            foreach (var special in specials)
            {
                if (Level >= special.Key)
                {
                    var list = GetSpecialsForLevelUp(special.Value);
                    Specials.AddRange(list);
                }
            }
            NextLevelExp = LevelFunction.NextLevel(Level);
            DoInitialLeveling();
        }

        public void Init(Enemy enemy)
        {
            if (enemy == null)
            {
                LogManager.LogError($"Null enemy passed to Actor[{name}]");
                return;
            }
            Spells.Clear();
            Specials.Clear();
            Id = actorId++;
            Name = ServiceManager.Get<LocalizationManager>().Localize(enemy.Name);
            Portrait = enemy.Portrait;
            Spells.AddRange(GetSpellsForLevelUp(enemy.Spells));
            Specials.AddRange(GetSpecialsForLevelUp(enemy.Specials));
            StealItem = enemy.StealItem;
            Loot = CreateLoot(enemy);
            Stats = new Stats(enemy.Stats, name);
        }

        private void DoInitialLeveling()
        {
            // Only party members need to level up
            if (!ServiceManager.Get<World>().Party.HasMemeber(PartyId))
                return;

            for (int i = 1; i < Level; i++)
                Exp += LevelFunction.NextLevel(i);
            Level = 0;
            NextLevelExp = LevelFunction.NextLevel(Level);
            while (ReadyToLevelUp())
            {
                var levelUp = CreateLevelUp();
                ApplyLevel(levelUp);
            }
        }

        public bool ReadyToLevelUp()
        {
            return Exp >= NextLevelExp;
        }

        public bool AddExp(int exp)
        {
            Exp += exp;
            return ReadyToLevelUp();
        }

        public LevelUp CreateLevelUp()
        {
            var levelUp = new LevelUp(NextLevelExp, 1, Stats);
            foreach (var growth in StatGrowth.Growths)
                levelUp.Stats.SetStat(growth.Key, growth.Value.RollDice());
            var level = Level + levelUp.Level;
            var partyDefinition = ServiceManager.Get<GameData>().PartyDefs[PartyId];
            var actionGrow = partyDefinition.ActionGrowth;
            if (actionGrow.Spells.ContainsKey(level))
                levelUp.Spells = GetSpellsForLevelUp(actionGrow.Spells[level]);
            if (actionGrow.Special.ContainsKey(level))
                levelUp.Specials = GetSpecialsForLevelUp(actionGrow.Special[level]);

            return levelUp;
        }

        public void ApplyLevel(LevelUp levelUp)
        {
            Exp += levelUp.Exp;
            Level += levelUp.Level;
            NextLevelExp = LevelFunction.NextLevel(Level);
            foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
                if (Stats.HasStat(stat))
                    Stats.IncreaseStat(stat, levelUp.Stats.Get(stat));
            Stats.ResetHpMp();
        }

        public void Equip(EquipSlot slot, ItemInfo item = null)
        {
            var previousItem = Equipment[(int)slot];
            Equipment[(int)slot] = null;
            if (previousItem != null)
            {
                Stats.RemoveModifier(slot);
                ServiceManager.Get<World>().AddItem(previousItem);
            }

            if (item == null)
                return;

            ServiceManager.Get<World>().RemoveItem(item.Id);
            Equipment[(int)slot] = item;
            Stats.AddModifier(slot, item.Stats.Modifier);
        }

        public void Unequip(EquipSlot slot)
        {
            Equip(slot);
        }

        public int EquipCount(ItemInfo item)
        {
            int count = 0;
            foreach (var _item in Equipment)
                if (_item.Id == item.Id)
                    count++;
            return count;
        }

        public string GetEquipmentName(EquipSlot slot)
        {
            var slotNumber = (int)slot;
            return Equipment[slotNumber] == null ? "--" : Equipment[slotNumber].GetName();
        }

        public ItemInfo GetEquipmentAtSlot(EquipSlot slot)
        {
            var slotNumber = (int)slot;
            return Equipment[slotNumber] != null ? Equipment[slotNumber] : null;
        }

        public List<int> PredictStats(EquipSlot slot, ItemInfo item)
        {
            var stats = new List<int>();
            var modifier = item == null ? null : item.Stats.Modifier;
            foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
                stats.Add(Stats.GetStatDiffForNewItem(stat, slot, modifier));
            return stats;
        }

        public bool CanUse(ItemInfo item)
        {
            foreach (var use in item.UseRestriction)
                if (use == UseRestriction.None || use == UseRestriction)
                    return true;
            return false;
        }

        public bool CanCast(Spell spell)
        {
            return spell.MpCost <= Stats.Get(Stat.MP);
        }

        public void ReduceManaForSpell(Spell spell)
        {
            if (!Stats.HasStat(Stat.MP))
            {
                LogManager.LogError($"{name} tried to use spell, but does not have MP stat!");
                return;
            }
            var mp = Stats.Get(Stat.MP);
            var cost = spell.MpCost;
            mp = Mathf.Max(mp - cost, 0);
            Stats.SetStat(Stat.MP, mp);
        }

        private List<Spell> GetSpellsForLevelUp(List<string> spells)
        {
            var gamedata = ServiceManager.Get<GameData>().Spells;
            var list = new List<Spell>();
            foreach (var spell in spells)
            {
                if (!gamedata.ContainsKey(spell))
                {
                    LogManager.LogError($"Gamedata does not contain Spell {spell}. Not adding to level up.");
                    continue;
                }
                list.Add(gamedata[spell]);
            }
            return list;
        }

        private List<Spell> GetSpecialsForLevelUp(List<string> specials)
        {
            var gamedata = ServiceManager.Get<GameData>().Specials;
            var list = new List<Spell>();
            foreach (var special in specials)
            {
                if (!gamedata.ContainsKey(special))
                {
                    LogManager.LogError($"Gamedata does not contain Special {special}. Not adding to level up.");
                    continue;
                }
                list.Add(gamedata[special]);
            }
            return list;
        }

        private Drop CreateLoot(Enemy enemy)
        {
            var gold = (int)UnityEngine.Random.Range(enemy.Gold.x, enemy.Gold.y);
            var always = new List<Item>();
            var oddments = new List<Oddment>();
            var items = ServiceManager.Get<GameData>().Items;
            foreach (var itemDrop in enemy.ItemDrops)
            {
                if (itemDrop.x < 0 || itemDrop.x > items.Count)
                {
                    LogManager.LogError($"ItemDrop has invalid item id: {itemDrop.x}.");
                    continue;
                }
                var item = items[(int)itemDrop.x] as ItemInfo;
                if (itemDrop.y < 0)
                    always.Add(new Item { ItemInfo = item, Count = (int)itemDrop.z });
                else
                {
                    var oddment = new Oddment
                    {
                        Item = item.Id,
                        Chance = (int)itemDrop.y,
                        Count = (int)itemDrop.z
                    };
                    oddments.Add(oddment);
                }
            }
            var chance = new OddmentTable();
            chance.SetOddments(oddments);
            var drop = new Drop
            { 
                Exp = enemy.Exp,
                Gold = gold,
                AlwaysDrop = always,
                ChanceDrop = chance
            };
            return drop;
        }
    }
}