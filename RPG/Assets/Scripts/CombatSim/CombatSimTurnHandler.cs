using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;

namespace RPG_CombatSim
{
    public class CombatSimTurnHandler : MonoBehaviour, ICombatTurnHandler
    {
        [SerializeField] EventQueue EventQueue;
        [SerializeField] CombatSim Sim;

        private ICombatState combat;

        public void Init(ICombatState state, EventQueue queue)
        {
            combat = state;
            EventQueue.Clear();
            AddTurns();
        }

        public void Execute()
        {
            EventQueue.Execute();
            if (EventQueue.IsEmpty())
                AddTurns();
        }

        public void ClearTurns()
        {
            EventQueue.Clear();
        }

        public void AddEvent(IEvent evt, int speed)
        {
            EventQueue.Add(evt, speed);
        }

        public void RemoveEventsForActor(int id)
        {
            EventQueue.RemoveEventsForActor(id);
        }

        private void AddTurns()
        {
            AddPlayerTurns(combat.GetPartyActors());
            AddEnemyTurns(combat.GetEnemyActors());
        }

        private void AddPlayerTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CESimTurn(actor, combat, Sim);
                    var speed = forceFirst ? firstSpeed : turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding turn for {actor.name}");
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
                    var turn = new CSEAITurn(actor, combat);
                    var speed = turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding AI turn for {actor.name}");
                }
            }
        }   
    }
}
