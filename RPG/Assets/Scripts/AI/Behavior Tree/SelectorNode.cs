using System.Collections.Generic;
using RPG_Character;

namespace RPG_AI
{
    public class SelectorNode : Node
    {
        private List<Node> nodes = new List<Node>();

        public SelectorNode(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running SelectorNode for Actor {actor}.");
            foreach (var node in nodes)
            {
                switch (node.Evaluate(actor))
                {
                    case NodeState.Running:
                        nodeState = NodeState.Running;
                        LogManager.LogDebug($"SelectorNode nodeState is {nodeState}.");
                        return nodeState;
                    case NodeState.Success:
                        nodeState = NodeState.Success;    
                        LogManager.LogDebug($"SelectorNode nodeState is {nodeState}.");
                        return nodeState;
                    case NodeState.Failure:
                    default:
                    break;
                }
            }
            nodeState = NodeState.Failure;
            LogManager.LogDebug($"SelectorNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}