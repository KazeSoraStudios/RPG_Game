using System.Collections.Generic;
using RPG_Character;
using RPG_Combat;
using RPG_CombatSim;

namespace RPG_AI
{
    public abstract class AttackNode : Node
    {
        private readonly bool sim;
        protected readonly ICombatState combat;

        public AttackNode(ICombatState combat)
        {
            this.combat = combat;
            sim = combat.IsSim();
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
            IEvent attackEvent;
            if(sim)
                attackEvent = new CSEAttack(config);
            else
                attackEvent = new CEAttack(config);
            var turnHandler = combat.CombatTurnHandler();
            turnHandler.AddEvent(attackEvent, -1);
        }

        protected void AddSpellEvent(CECastSpellEvent.Config config)
        {
            LogManager.LogDebug($"Adding CECastSpellEvent for {config.Actor.name}");
            IEvent spellEvent;
            if(sim)
                spellEvent = new CSECastSpellEvent(config);
            else
                spellEvent = new CECastSpellEvent(config);
            var turnHandler = combat.CombatTurnHandler();
            turnHandler.AddEvent(spellEvent, -1);
        }
    }
}