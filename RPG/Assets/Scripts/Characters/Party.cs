using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    private Dictionary<int, Actor> members = new Dictionary<int, Actor>();

    public void Reset()
    {
        members.Clear();
    }

    public bool HasMemeber(int id)
    {
        return members.ContainsKey(id);
    }

    public Actor GetActor(int id)
    {
        if (!HasMemeber(id))
        {
            LogManager.LogError($"Actor not found at party slot {id}");
            return null;
        }
        return members[id];
    }

    public void Add(Actor member)
    {
        if (members.ContainsKey(member.Id))
            return;
        members.Add(member.Id, member);
    }
    
    public void RemoveById(int id)
    {
        members.Remove(id);
    }
    
    public Actor[] ToArray()
    {
        var actors = new Actor[members.Count];
        int index = 0;
        foreach (var member in members)
            actors[index++] = member.Value;
        return actors;
    }

    public int EquipCount(Item item)
    {
        int count = 0;
        foreach (var member in members)
            count += member.Value.EquipCount(item);
        return count;
    }

    public void Rest()
    {
        foreach(var member in members)
            if (member.Value.Stats.Get(Stat.HP) > 0)
                member.Value.Stats.ResetHpMp();
    }

    public void RestAll()
    {
        foreach (var member in members)
            member.Value.Stats.ResetHpMp();
    }

    public void DebugPrintParty()
    {
        foreach (var member in members)
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
