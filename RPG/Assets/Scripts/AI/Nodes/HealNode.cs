using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public class HealNode : Node
    {
        public HealNode() { }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running HealNode for Actor {actor.name}.");
            nodeState = actor.TryHeal() ? NodeState.Success : NodeState.Failure;
            LogManager.LogDebug($"HealNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
