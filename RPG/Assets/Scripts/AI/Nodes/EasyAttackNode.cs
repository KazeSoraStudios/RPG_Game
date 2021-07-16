using System.Collections.Generic;
using RPG_Character;
using RPG_Combat;

namespace RPG_AI
{
    public class EasyAttackNode : AttackNode
    { 
        public EasyAttackNode(CombatGameState combat) : base (combat) {}
        public override NodeState Evaluate(Actor actor)
        {
            LogManager.LogDebug($"Running EasyAttackNode for Actor {actor.name}.");
            var targets = new List<Actor>();
            targets.AddRange(CombatSelector.RandomPlayer(combat));
            var config = new CEAttack.Config
            {
                IsCounter = false,
                IsPlayer = false,
                Actor = actor,
                CombatState = combat,
                Targets = targets
            };
            AddAttackEvent(config);
            nodeState = NodeState.Success;
            LogManager.LogDebug($"EasyAttackNode nodeState is {nodeState}.");
            return nodeState;
        }
    }
}
