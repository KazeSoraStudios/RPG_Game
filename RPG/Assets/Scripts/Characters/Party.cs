using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    public Dictionary<int, Actor> Members = new Dictionary<int, Actor>();

    public void Reset()
    {
        Members.Clear();
    }

    public bool HasMemeber(Actor actor)
    {
        return HasMemeber(actor.Id);
    }

    public bool HasMemeber(int id)
    {
        return Members.ContainsKey(id);
    }

    public void Add(Actor member)
    {
        if (Members.ContainsKey(member.Id))
            return;
        Members.Add(member.Id, member);
    }
    
    public void RemoveById(int id)
    {
        Members.Remove(id);
    }

    public void Remove(Actor member)
    {
        Members.Remove(member.Id);
    }
    
    public Actor[] ToArray()
    {
        var actors = new Actor[Members.Count];
        int index = 0;
        foreach (var member in Members)
            actors[index++] = member.Value;
        return actors;
    }

    public int EquipCount(Item item)
    {
        int count = 0;
        foreach (var member in Members)
            count += member.Value.EquipCount(item);
        return count;
    }

    public void Rest()
    {
        foreach(var member in Members)
            if (member.Value.Stats.Get(Stat.HP) > 0)
                member.Value.Stats.ResetHpMp();
    }

    public void RestAll()
    {
        foreach (var member in Members)
            member.Value.Stats.ResetHpMp();
    }

    public void DebugPrintParty()
    {
        foreach (var member in Members)
        {
            var actor = member.Value;
            var name = actor.name;
            var stats = actor.Stats;
            var hp = stats.Get(Stat.HP);
            var maxHp = stats.Get(Stat.MaxHP);
            LogManager.LogDebug($"{name}: , hp: {hp}/{maxHp}");
        }
    }
}
