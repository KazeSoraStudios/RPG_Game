using System.Collections.Generic;
using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public abstract class AttackNode : Node
    {
        protected readonly CombatGameState combat;

        public AttackNode(CombatGameState combat)
        {
            this.combat = combat;
        }

        protected List<Actor> GetTargets(float chance)
        {
            var attack = UnityEngine.Random.Range(0.0f, 1.1f);
            if (attack <= chance)
                return CombatSelector.FindWeakestEnemy(combat);
            else
                return CombatSelector.RandomPlayer(combat);
        }

        protected void AddAttackEvent(CEAttack.Config config)
        {
            LogManager.LogDebug($"Adding CEAttackEvent for {config.Actor.name}");
            var attackEvent = new CEAttack(config);
            var queue = combat.EventQueue;
            var priority = attackEvent.CalculatePriority(queue);
            queue.Add(attackEvent, -1);
        }

        protected void AddSpellEvent(CECastSpellEvent.Config config)
        {
            LogManager.LogDebug($"Adding CECastSpellEvent for {config.Actor.name}");
            var spellEvent = new CECastSpellEvent(config);
            var queue = combat.EventQueue;
            var priority = spellEvent.CalculatePriority(queue);
            queue.Add(spellEvent, -1);
        }
    }
}