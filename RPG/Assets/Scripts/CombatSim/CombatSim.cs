using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_AI;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_CombatSim
{
    public class CombatSim : MonoBehaviour
    {
        [SerializeField] EventQueue EventQueue;
        public Dictionary<AIType, Node> BehaviorTrees = new Dictionary<AIType, Node>();

        public Node GetBehaviorTreeForAIType(AIType type)
        {
            return BehaviorTrees.ContainsKey(type) ?
                BehaviorTrees[type] : null;
        }

        private void AddPlayerTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    // var turn = new CETurn(actor, this);
                    // var speed = forceFirst ? firstSpeed : turn.CalculatePriority(EventQueue);
                    // EventQueue.Add(turn, speed);
                    // LogManager.LogDebug($"Adding turn for {actor.name}");
                }
            }
        }

        private void AddEnemyTurns(List<Actor> actors)
        {
            foreach (var actor in actors)
            {
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    // var turn = new CEAITurn(actor, this);
                    // var speed = turn.CalculatePriority(EventQueue);
                    // EventQueue.Add(turn, speed);
                    // LogManager.LogDebug($"Adding AI turn for {actor.name}");
                }
            }
        }
    }
}
