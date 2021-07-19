using RPG_Character;

namespace RPG_AI
{
    public class InverterNode : Node
    {
        public Node node;
        
        public InverterNode(Node node)
        {
            this.node = node;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running InverterNode for Actor {actor.name}.");
            switch (node.Evaluate(actor))
            {
                case NodeState.Running:
                    nodeState = NodeState.Running;
                    break;
                case NodeState.Success:
                    nodeState = NodeState.Failure;
                    break;
                case NodeState.Failure:
                    nodeState = NodeState.Success;
                    break;
                default:
                break;
            }
            LogManager.LogDebug($"InverterNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
