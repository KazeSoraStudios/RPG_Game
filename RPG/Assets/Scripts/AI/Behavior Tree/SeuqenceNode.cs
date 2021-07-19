using System.Collections.Generic;
using RPG_Character;

namespace RPG_AI
{
    public class SeuqenceNode : Node
    {
        private List<Node> nodes = new List<Node>();

        public SeuqenceNode(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running SequenceNode for Actor {actor.name}.");
            bool isAnyNodeRunning = false;
            foreach (var node in nodes)
            {
                switch (node.Evaluate(actor))
                {
                    case NodeState.Running:
                        isAnyNodeRunning = true;
                        break;
                    case NodeState.Failure:
                        nodeState = NodeState.Failure;
                        return nodeState;
                    case NodeState.Success:
                    default:
                    break;
                }
            }
            nodeState = isAnyNodeRunning ? NodeState.Running : NodeState.Success;
            LogManager.LogDebug($"SequenceNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
