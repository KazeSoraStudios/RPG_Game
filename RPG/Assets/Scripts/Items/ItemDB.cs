using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;
public enum ItemType { Useable, Key, Accessory, Weapon, Armor, None };
public enum UseRestriction { None, Hero, Thief, Mage };

public class ItemDB : MonoBehaviour
{
    //[SerializeField] public Dictionary<int, Item> items = new Dictionary<int, Item>();
    ////[SerializeField] private List<ItemInfo> Items;
    //[SerializeField] static int NumberOfItems;
    //[SerializeField] static int NumberOfKeyItems;


    //public static int GetNumberOfItems()
    //{
    //    return NumberOfItems;
    //}

    //public static int GetNumberOfKeyItems()
    //{
    //    return NumberOfKeyItems;
    //}
}

public class Item
{
    public ItemInfo ItemInfo;
    public int Count;

    public Item() { }

    public Item(ItemInfo info, int count)
    {
        ItemInfo = info;
        Count = count;
    }
}

public class ItemInfo
{
    public int Index;
    public int Price;
    public ItemType Type;
    public string Id;
    public string Name;
    public string Description;
    public string Icon;
    public string Use;
    public UseRestriction[] UseRestriction;
    public ItemStats Stats;
    public StatModifier Modifier = new StatModifier();
    // use Action

    public string GetName()
    {
        return ServiceManager.Get<LocalizationManager>().Localize(Id);
    }
}

public class ItemUse
{
    public string Id;

    public int Amount;

    public string Hint;

    public string Action;

    public bool UseOnMap;

    public bool SwitchSides;

    public ItemTarget Target;
}

public class ItemTarget
{
    public Func<CombatGameState, bool, List<Actor>> Selector;

    public bool SwitchSides;

    public CombatTargetType Type;
}

public class ItemStats
{
    public StatModifier Modifier = new StatModifier();

    public ItemStats(string data)
    {
        if (data.IsEmpty())
            return;
        var types = data.Split(':');
        if (types.Length < 1)
            return;

        for (int i = 0; i < types.Length; i++)
        {
            if (types[i].Equals("add"))
            {
                SetUpAddModifier(types[++i]);
            }
            else
            {
                SetUpMultiplyModifier(types[++i]);
            }
        }
    }

    private void SetUpAddModifier(string data)
    {
        if (data.IsEmpty())
        {
            LogManager.LogError($"passed to SetUpAddModifier is empty.");
            return;
        }
        var modifiers = data.Split('/');
        foreach(var modifier in modifiers)
        {
            var stats = modifier.Split(';');
            if (stats.Length != 2)
            {
                LogManager.LogError($"Stats has incorrect parameters{stats}. Not adding modifier.");
                continue;
            }
            if (!Enum.TryParse<Stat>(stats[0], true, out var stat))
            {
                LogManager.LogError($"Unable to parse {stats[0]} to Stat. Not adding modifier.");
                continue;
            }
            if (!int.TryParse(stats[1], out var amount))
            {
                LogManager.LogError($"Unable to parse {stats[1]} to int. Not adding modifier.");
                continue;
            }
            Modifier.AddModifiers.Add(stat,amount);
        }
    }

    private void SetUpMultiplyModifier(string data)
    {
        if (data.IsEmpty())
        {
            LogManager.LogError($"passed to SetUpMultiplyModifier is empty.");
            return;
        }
        var modifiers = data.Split('/');
        foreach (var modifier in modifiers)
        {
            var stats = modifier.Split(';');
            if (stats.Length != 2)
            {
                LogManager.LogError($"Stats has incorrect parameters{stats}. Not adding modifier.");
                continue;
            }
            if (!Enum.TryParse<Stat>(stats[0], true, out var stat))
            {
                LogManager.LogError($"Unable to parse {stats[0]} to Stat. Not adding modifier.");
                continue;
            }
            if (!float.TryParse(stats[1], out var amount))
            {
                LogManager.LogError($"Unable to parse {stats[1]} to float. Not adding modifier.");
                continue;
            }
            Modifier.MultiplyModifiers.Add(stat, amount);
        }
    }
}
