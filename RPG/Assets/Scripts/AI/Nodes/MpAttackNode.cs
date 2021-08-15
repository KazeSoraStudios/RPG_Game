using System.Collections.Generic;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_AI
{
    public class MpAttackNode : AttackNode
    {
        private float specialChance = 0.5f;
        private float magicChance = 0.5f;
        private float attackWeakestChance = 0.6f;
        private float randomAttackChance = 0.4f;
        private Actor currentActor;

        public MpAttackNode(ICombatState combat) : base(combat) { }

        public MpAttackNode(ICombatState combat, 
            float specialChance = 0.5f, float magicChance = 0.5f, float attackWeakest = 0.6f, float attackRandom = 0.4f) : base (combat) 
        {
            this.specialChance = specialChance;
            this.magicChance = magicChance;
            this.attackWeakestChance = attackWeakest;
            this.randomAttackChance = attackRandom;
        }

        public override NodeState Evaluate(Actor actor)
        {
            currentActor = actor;
            LogManager.LogDebug($"Running MpAttackNode for Actor {currentActor.name}.");
            var action = UnityEngine.Random.Range(0.0f, 1.1f);
            LogManager.LogDebug($"MpAttackNode action is {action}, maigc: {magicChance}, special: {specialChance}");
            if (action <= magicChance && TryAction(true))
                nodeState = NodeState.Success;
            else if (TryAction(false))
                nodeState = NodeState.Success;
            else
                nodeState = NodeState.Failure;
            LogManager.LogDebug($"MpAttackNode nodeState is {nodeState}");
            currentActor = null;
            return nodeState;
        }

        private bool TryAction(bool magicFirst)
        {
            LogManager.LogDebug($"Trying Action (magic first: {magicFirst} for {currentActor.name}");
            return magicFirst ? TryAction(currentActor.Spells) || TryAction(currentActor.Specials) :
                TryAction(currentActor.Specials) || TryAction(currentActor.Spells);

        }

        private bool TryAction(List<Spell> spells)
        {
            while (spells.Count > 0)
            {
                var index = UnityEngine.Random.Range(0, spells.Count);
                var spell = spells[index];
                if (currentActor.CanCast(spell))
                {
                    CastSpell(spell);
                    return true;
                }
                spells.RemoveAt(index);
            }
            LogManager.LogDebug($"Cannot cast spell for {currentActor.name}");
            return false;
        }

        private void CastSpell(Spell spell)
        {
            var config = new CECastSpellEvent.Config
            { 
                Actor = currentActor,
                CombatState = combat,
                Spell = spell,
                IsPlayer = false,
                Targets = GetTargets(attackWeakestChance)
            };
            AddSpellEvent(config);
        }
    }
}
