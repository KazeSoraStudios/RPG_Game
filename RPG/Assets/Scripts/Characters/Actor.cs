using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public struct LevelUp
    {
        public int Exp;
        public int Level;
        public Stats Stats;
        //Actions

        public LevelUp(int exp, int level, Stats stats)
        {
            Exp = exp;
            Level = level;
            Stats = stats;
        }
    }
    public enum EquipSlot { Weapon, Armor, Accessory1, Accessory2 };

    [SerializeField] public string Name;
    [SerializeField] public string Portrait;
    [SerializeField] public int Id;
    [SerializeField] public int Exp;
    [SerializeField] public int Level;
    [SerializeField] public int NextLevelExp;
    [SerializeField] public UseRestriction UseRestriction;
    [SerializeField] public List<Spell> Spells;
    //[SerializeField] MenuActions MenuActions SerializeObject
    [SerializeField] public Stats Stats;
    [SerializeField] public LevelFunction LevelFunction;
    [SerializeField] public StatGrowth StatGrowth = new StatGrowth();
    [SerializeField] public ItemInfo[] Equipment = new ItemInfo[3];
    [SerializeField] public List<Item> Loot = new List<Item>();

    public void Init(PartyMemeberDefintion partyMemeber)
    {
        if (partyMemeber == null)
        {
            LogManager.LogError($"Null party member passed to Actor[{name}]");
            return;
        }
        Stats.FromStatsDefinition(ServiceManager.Get<GameData>().Stats[partyMemeber.StatsId]);
        StatGrowth = partyMemeber.StatGrowth;
        Portrait = partyMemeber.Portrait;
        Name = ServiceManager.Get<LocalizationManager>().Localize(partyMemeber.Name);
        Level = partyMemeber.Level;
        // TODO action growth

        NextLevelExp = LevelFunction.NextLevel(Level);
        DoInitialLeveling();
    }

    /*
        mMagic = ShallowClone(def.magic or {}),
        mSpecial = ShallowClone(def.special or {}),
        mStealItem = def.steal_item,
    }
    */



    /*

    if def.drop then
        local drop = def.drop
        local goldRange = drop.gold or {}
        local gold  = math.random(goldRange[0] or 0, goldRange[1] or 0)

        this.mDrop =
        {
            mXP = drop.xp or 0,
            mGold = gold,
            mAlways = drop.always or {},
            mChance = OddmentTable:Create(drop.chance)
        }

    end
        
     */

    private void DoInitialLeveling()
    {
        // Only party members need to level up
        if (!ServiceManager.Get<World>().Party.HasMemeber(Id))
            return;

        for (int i = 1; i < Level; i++)
            Exp += LevelFunction.NextLevel(i);
        Level = 0;
        NextLevelExp = LevelFunction.NextLevel(Level);
        while(ReadyToLevelUp())
        {
            var levelUp = CreateLevelUp();
            ApplyLevel(levelUp);
        }
    }

    private bool ReadyToLevelUp()
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
        //var actions 
        // TODO Get party member and check actions

        //local def = gPartyMemberDefs[self.mId]
        //levelup.actions = def.actionGrowth[level] or { }

        return levelUp;
    }

    //function Actor:UnlockMenuAction(id)

    //print('menu action', tostring(id))

    //for _, v in ipairs(self.mActions) do
    //    if v == id then
    //        return
    //    end
    //end
    //table.insert(self.mActions, id)
    //end

    //function Actor:AddAction(action, entry)

    //    local t = self.mSpecial
    //    if action == 'magic' then
    //        t = self.mMagic
    //    end

    //    for _, v in ipairs(entry) do
    //        table.insert(t, v)
    //    end
    //end

    public void ApplyLevel(LevelUp levelUp)
    {
        Exp += levelUp.Exp;
        Level += levelUp.Level;
        NextLevelExp = LevelFunction.NextLevel(Level);

        //assert(self.mXP >= 0)
        
        foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
            if (Stats.HasStat(stat))
                Stats.IncreaseStat(stat, levelUp.Stats.Get(stat));


        // TODO
        //for action, v in pairs(levelup.actions) do
        //    self:UnlockMenuAction(action)
        //    self:AddAction(action, v)
        //end
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
        Equipment[(int)slot] = item; // TODO change to use ids and get from db
        Stats.AddModifier(slot, item.Modifier);
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

//function Actor:CreateStatNameList()

    //    local list = { }

    //    for _, v in ipairs(Actor.ActorStats) do
    //        table.insert(list, v)
    //    end

    //    for _, v in ipairs(Actor.ItemStats) do
    //        table.insert(list, v)
    //    end


    //    table.insert(list, "hp_max")
    //    table.insert(list, "mp_max")

    //    return list

    //end

    //function Actor:CreateStatLabelList()
    //    local list = { }

    //    for _, v in ipairs(Actor.ActorStatLabels) do
    //        table.insert(list, v)
    //    end

    //    for _, v in ipairs(Actor.ItemStatLabels) do
    //        table.insert(list, v)
    //    end


    //    table.insert(list, "HP:")
    //    table.insert(list, "MP:")

    //    return list
    //end

    public List<int> PredictStats(EquipSlot slot, ItemInfo item)
    {
        var stats = new List<int>();
        foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
            stats.Add(Stats.GetStatDiffForNewItem(stat, slot, item.Modifier));
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
}
