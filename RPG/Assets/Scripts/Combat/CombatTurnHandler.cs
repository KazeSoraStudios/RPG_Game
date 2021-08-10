using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public interface ICombatTurnHandler
    {
        void Init(ICombatState state, EventQueue queue);
        void Execute();
        public void ClearTurns();
        public void AddEvent(IEvent evt, int speed);
        public void RemoveEventsForActor(int id);
    }
    public class CombatTurnHandler : MonoBehaviour, ICombatTurnHandler
    {
        private EventQueue eventQueue;
        private ICombatState combatState;

        public void Init(ICombatState state, EventQueue queue)
        {
            combatState = state;
            eventQueue = queue;
            eventQueue.Clear();
            AddTurns();
        }

        public void Execute()
        {
            eventQueue.Execute();
            if (eventQueue.IsEmpty())
                AddTurns();
        }

        public void ClearTurns()
        {
            eventQueue.Clear();
        }

        public void AddEvent(IEvent evt, int speed)
        {
            eventQueue.Add(evt, speed);
        }

        public void RemoveEventsForActor(int id)
        {
            eventQueue.RemoveEventsForActor(id);
        }

        private void AddTurns()
        {
            AddPlayerTurns(combatState.GetPartyActors());
            AddEnemyTurns(combatState.GetEnemyActors());
        }

        private void AddPlayerTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !eventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CETurn(actor, combatState);
                    var speed = forceFirst ? firstSpeed : turn.CalculatePriority(eventQueue);
                    eventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding turn for {actor.name}");
                }
            }
        }

        private void AddEnemyTurns(List<Actor> actors)
        {
            foreach (var actor in actors)
            {
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !eventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CEAITurn(actor, combatState);
                    var speed = turn.CalculatePriority(eventQueue);
                    eventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding AI turn for {actor.name}");
                }
            }
        }
    }
}
