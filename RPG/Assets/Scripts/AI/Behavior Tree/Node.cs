namespace RPG_AI
{
    public enum NodeState { Running, Success, Failure }

    [System.Serializable]
    public abstract class Node
    {
        protected NodeState nodeState;
        public NodeState NodeState { get { return nodeState; } }
        public abstract NodeState Evaluate(RPG_Character.Actor actor);
    }
}