using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

public sealed class GameData : MonoBehaviour
{
    public Dictionary<string, ItemInfo> Items = new Dictionary<string, ItemInfo>();
    public Dictionary<string, ItemUse> ItemUses = new Dictionary<string, ItemUse>();
    public Dictionary<string, PartyMemeberDefintion> PartyDefs = new Dictionary<string, PartyMemeberDefintion>();
    public Dictionary<string, Dictionary<Stat, int>> Stats = new Dictionary<string, Dictionary<Stat, int>>();
    public Dictionary<string, Spell> Spells = new Dictionary<string, Spell>();

    void Awake()
    {
        ServiceManager.Register(this);
    }

    void OnDestroy()
    {
        LogManager.LogWarn("GameData shutting down. Is game finished?");
        ServiceManager.Unregister(this);
    }

    public Dictionary<Stat, int> GetStatsForActor(string actor)
    {
        return new Dictionary<Stat, int>();
    }
}

public class PartyMemeberDefintion
{
    public int Level;
    public string Id;
    public string Name;
    public string Portrait;
    public string StatsId;
    public StatGrowth StatGrowth;
    public ActionGrowth ActionGrowth;

    public string LocalizedName()
    {
        return ServiceManager.Get<LocalizationManager>().Localize(Name);
    }
}

public class ActionGrowth
{
    public Dictionary<int, List<string>> Spells = new Dictionary<int, List<string>>();
    public Dictionary<int, List<string>> Special = new Dictionary<int,List<string>>();
}

public enum SpellElement { Fire, Ice, Electric, Heal }

public class Spell
{
    public string Id;
    public string Name;
    public string Description;
    public string Action;
    public SpellElement SpellElement;
    public int MpCost;
    public Vector2 BaseDamage;
    public float HitChance;
    public ItemTarget ItemTarget;

    public string LocalizedName()
    {
        return ServiceManager.Get<LocalizationManager>().Localize(Name);
    }
}