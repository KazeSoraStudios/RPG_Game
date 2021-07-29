using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public class MediumAttackNode : AttackNode
    {
        private float attackWeakestChance = 0.6f;
        private float randomAttackChance = 0.4f;

        public MediumAttackNode(ICombatState combat) : base (combat) {}
        public MediumAttackNode(ICombatState combat, 
            float attackWeakest = 0.6f, float attackRandom = 0.4f) : base (combat) 
        {
            this.attackWeakestChance = attackWeakest;
            this.randomAttackChance = attackRandom;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running MediumAttackNode for Actor {actor.name}.");
            MeleeAttack(actor);
            nodeState = NodeState.Success;
            LogManager.LogDebug($"MediumAttackNode nodeState is {nodeState}");
            return nodeState;
        }

        private void MeleeAttack(Actor actor)
        {
            var config = new CEAttack.Config
            {
                IsCounter = false,
                IsPlayer = false,
                Actor = actor,
                CombatState = combat,
                Targets = GetTargets(attackWeakestChance)
            };
            AddAttackEvent(config);
        }
    }
}