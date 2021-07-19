using RPG_Combat;
using RPG_Character;

namespace RPG_AI
{
    public class AITurnNode : Node
    {
        private readonly CombatGameState combat;
        private Node baseNode;

        public AITurnNode(CombatGameState state, Node node)
        {
            this.combat = state;
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