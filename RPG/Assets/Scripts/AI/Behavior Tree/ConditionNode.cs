using RPG_Character;

namespace RPG_AI
{
    public class ConditionNode : Node
    {
        private readonly Node condition;
        private readonly Node success;

        public ConditionNode(Node condition, Node success)
        {
            this.condition = condition;
            this.success = success;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running ConditionNode for Actor {actor.name}.");
            var succeeded = condition.Evaluate(actor) == NodeState.Success;
            nodeState = succeeded ? success.Evaluate(actor) : NodeState.Failure;
            LogManager.LogDebug($"ConditionNode condition result is {succeeded}, nodeState is {nodeState}.");
            return nodeState;
        }
    }
}