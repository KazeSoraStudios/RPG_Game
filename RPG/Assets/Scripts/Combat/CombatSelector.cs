using System.Collections.Generic;
using UnityEngine;

public enum Selector { WeakestActor, LowestHP, WeakestEnemy, WeakestPartyMember, LowestEnemyHP, LowestPartyHP, LowestMPParty, LowestMPEnemy, FirstDeadParty, FirstDeadEnemy, Random }

public sealed class CombatSelector
{
    public static Actor FindWeakestActor(List<Actor> actors, bool hurtOnly)
    {
        // TODO empty actor if null/empty
        Actor target = actors[0];
        int health = int.MaxValue;
        foreach(var actor in actors)
        {
            var hp = actor.Stats.Get(Stat.HP);
            var maxHp = actor.Stats.Get(Stat.MaxHP);
            var isHurt = hp < maxHp;
            bool skip = hurtOnly && isHurt;
            if (hp < health && !skip)
            {
                target = actor;
                health = hp;
            }
        }
        return target;
    }

    public static Actor FindLowestRemainingMP(List<Actor> actors, bool hurtOnly)
    {
        // TODO empty actor if null/empty
        Actor target = actors[0];
        int magic = int.MaxValue;
        foreach (var actor in actors)
        {
            var mp = actor.Stats.Get(Stat.MP);
            var maxMp = actor.Stats.Get(Stat.MaxMP);
            var isHurt = mp < maxMp;
            bool skip = hurtOnly && isHurt;
            if (mp < magic && !skip)
            {
                target = actor;
                magic = mp;
            }
        }
        return target;
    }

    public static Actor FindWeakestEnemy(CombatState state)
    {
        return FindWeakestActor(state.GetEnemiesActors(), false);
    }

    public static Actor FindWeakestHurtEnemy(CombatState state)
    {
        return FindWeakestActor(state.GetEnemiesActors(), true);
    }

    public static Actor FindWeakestPartyMember(CombatState state)
    {
        return FindWeakestActor(state.GetPartyActors(), false);
    }

    public static Actor FindWeakestHurtPartyMember(CombatState state)
    {
        return FindWeakestActor(state.GetPartyActors(), true);
    }

    public static Actor FindLowestMPPartyMember(CombatState state)
    {
        return FindLowestRemainingMP(state.GetPartyActors(), true);
    }

    public static Actor FirstDeadPartyMember(CombatState state)
    {
        var actors = state.GetPartyActors();
        foreach (var actor in actors)
        {
            var hp = actor.Stats.Get(Stat.HP);
            if (hp <= 0)
                return actor;
        }
        // TODO return empty
        return actors[0];
    }

    public static List<Actor> Party(CombatState state)
    {
        return state.GetEnemiesActors();
    }


    public static List<Actor> Enemies(CombatState state)
    {
        return state.GetPartyActors();
    }

    public static List<Actor> SelectAll(CombatState state)
    {
        var actors = new List<Actor>(state.GetPartyActors());
        actors.AddRange(state.GetEnemiesActors());
        return actors;
    }
    
    public static Actor RandomPlayer(CombatState state)
    {
        var actors = state.GetPartyActors();
        var aliveActors = new Actor[actors.Count];
        int index = 0;
        foreach(var actor in actors)
        {
            var hp = actor.Stats.Get(Stat.HP);
            if (hp > 0)
                aliveActors[index++] = actor;
        }
        return aliveActors[Random.Range(0, index)];
    }
}