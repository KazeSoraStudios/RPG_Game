using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public class CombatTurnHandler : MonoBehaviour
    {
        [SerializeField] EventQueue EventQueue;
        private ICombatState combatState;

        public void Init(ICombatState state)
        {
            combatState = state;
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
            AddPlayerTurns(combatState.GetPartyActors());
            AddEnemyTurns(combatState.GetEnemyActors());
        }

        private void AddPlayerTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CETurn(actor, combatState);
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
                    var turn = new CEAITurn(actor, combatState);
                    var speed = turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding AI turn for {actor.name}");
                }
            }
        }
    }
}
