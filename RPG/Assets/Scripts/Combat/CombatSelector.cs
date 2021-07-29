using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public enum Selector { WeakestActor, LowestHP, WeakestEnemy, WeakestPartyMember, LowestEnemyHP, 
        LowestPartyHP, LowestMPParty, LowestMPEnemy, FirstDeadParty, FirstDeadEnemy, RandomParty, RandomEnemy,
        FullParty, FullEnemies }

    public sealed class CombatSelector
    {
        public static List<Actor> FindWeakestActor(List<Actor> actors, bool hurtOnly)
        {
            DebugAssert.Assert(actors != null && actors.Count > 0, "Actor List passed to FindWeakestActor but list has no memebers.");
            var target = actors[0];
            int health = int.MaxValue;
            foreach (var actor in actors)
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
            return new List<Actor> { target };
        }

        public static List<Actor> FindLowestRemainingMP(List<Actor> actors, bool hurtOnly)
        {
            DebugAssert.Assert(actors != null && actors.Count > 0, "Actor List passed to FindLowestRemainingMP but list has no memebers.");
            var target = actors[0];
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
            return new List<Actor> { target };
        }

        public static List<Actor> FindWeakestEnemy(ICombatState state)
        {
            return FindWeakestActor(state.GetEnemyActors(), false);
        }

        public static List<Actor> FindWeakestHurtEnemy(ICombatState state)
        {
            return FindWeakestActor(state.GetEnemyActors(), true);
        }

        public static List<Actor> FindWeakestPartyMember(ICombatState state)
        {
            return FindWeakestActor(state.GetPartyActors(), false);
        }

        public static List<Actor> FindWeakestHurtPartyMember(ICombatState state)
        {
            return FindWeakestActor(state.GetPartyActors(), true);
        }

        public static List<Actor> FindLowestMPPartyMember(ICombatState state)
        {
            return FindLowestRemainingMP(state.GetPartyActors(), true);
        }

        public static List<Actor> FindLowestMPEnemy(ICombatState state)
        {
            return FindLowestRemainingMP(state.GetEnemyActors(), true);
        }

        public static List<Actor> FirstDeadPartyMember(ICombatState state)
        {
            var actors = state.GetPartyActors();
            foreach (var actor in actors)
            {
                var hp = actor.Stats.Get(Stat.HP);
                if (hp <= 0)
                    return new List<Actor> { actor };
            }
            return new List<Actor> { actors[0] };
        }

        public static List<Actor> FirstDeadEnemy(ICombatState state)
        {
            var actors = state.GetEnemyActors();
            foreach (var actor in actors)
            {
                var hp = actor.Stats.Get(Stat.HP);
                if (hp <= 0)
                    return new List<Actor> { actor };
            }
            return new List<Actor> { actors[0] };
        }


        public static List<Actor> Party(ICombatState state)
        {
            return state.GetPartyActors();
        }


        public static List<Actor> Enemies(ICombatState state)
        {
            return state.GetEnemyActors();
        }

        public static List<Actor> SelectAll(ICombatState state)
        {
            var actors = new List<Actor>(state.GetPartyActors());
            actors.AddRange(state.GetEnemyActors());
            return actors;
        }

        public static List<Actor> RandomPlayer(ICombatState state)
        {
            var actors = state.GetPartyActors();
            var aliveActors = new Actor[actors.Count];
            int index = 0;
            foreach (var actor in actors)
            {
                var hp = actor.Stats.Get(Stat.HP);
                if (hp > 0)
                    aliveActors[index++] = actor;
            }
            return new List<Actor> { aliveActors[Random.Range(0, index)] };
        }

        public static List<Actor> RandomEnemy(ICombatState state)
        {
            var actors = state.GetEnemyActors();
            var aliveActors = new Actor[actors.Count];
            int index = 0;
            foreach (var actor in actors)
            {
                var hp = actor.Stats.Get(Stat.HP);
                if (hp > 0)
                    aliveActors[index++] = actor;
            }
            return new List<Actor> { aliveActors[Random.Range(0, index)] };
        }
    }
}