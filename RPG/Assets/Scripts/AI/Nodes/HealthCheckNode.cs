using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public class HealthCheckNode : Node
    {
        public readonly float threshold;
        private CombatGameState combat;

        public HealthCheckNode(float threshold, CombatGameState combat)
        {
            this.threshold = threshold;
            this.combat = combat;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running HealthCheckNode for Actor {actor.name}, threshold is {threshold}.");
            var limit = actor.Stats.Get(Stat.MaxHP) * threshold;
            var hp = actor.Stats.Get(Stat.HP);
            nodeState = hp <= threshold ? NodeState.Success : NodeState.Failure;
            LogManager.LogDebug($"HealthCheckNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
