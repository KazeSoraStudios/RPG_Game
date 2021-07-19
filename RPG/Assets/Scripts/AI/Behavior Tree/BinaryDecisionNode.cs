using System;
using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public class BinaryDecisionNode : Node
    {
        private readonly float threshold;
        private readonly Node nodeA;
        private readonly Node nodeB;
        private readonly Func<Actor, bool> condition;

        public BinaryDecisionNode(float threshold, Node nodeA, Node nodeB, Func<Actor, bool> condition)
        {
            this.threshold = threshold;
            this.nodeA = nodeA;
            this.nodeB = nodeB;
            this.condition = condition;
        }

        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running BinaryDecisionNode for Actor {actor.name}.");
            var value = UnityEngine.Random.Range(0, 1.0f);
            var runANode = value <= threshold && condition(actor);
            nodeState = runANode ? nodeA.Evaluate(actor) : nodeB.Evaluate(actor);
            LogManager.LogDebug($"BinaryDecisionNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
