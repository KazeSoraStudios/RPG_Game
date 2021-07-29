using RPG_Combat;
using RPG_Character;

namespace RPG_AI
{
    public class AITurnNode : Node
    {
        private Node baseNode;

        public AITurnNode(Node node)
        {
            this.baseNode = node;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running AITurnNode for Actor {actor.name}.");
            nodeState = baseNode.Evaluate(actor);
            LogManager.LogDebug($"AITurnNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}