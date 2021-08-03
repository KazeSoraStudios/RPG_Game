using System;
using System.Collections.Generic;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_AI
{
    public enum AIType { Easy, Medium, Hard, Magic, Healer, Melee }
    public class AIBehaviors
    {
        public static Dictionary<AIType, Node> LoadEnemyBehaviors(ICombatState state, List<Actor> enemies)
        {
            var ai = new Dictionary<AIType, Node>();
            foreach (var enemy in enemies)
            {
                var enemyAI = ServiceManager.Get<GameData>().EnemyAI;
                if (!enemyAI.ContainsKey(enemy.GameDataId))
                {
                    LogManager.LogError($"Enemy {enemy.name}, ID: {enemy.GameDataId} is not found in EnemyAI.");
                    continue;
                }
                var aiData = enemyAI[enemy.GameDataId];
                if (ai.ContainsKey(aiData.Type))
                    continue;
                var node = LoadBehaviorTree(state, aiData);
                ai[aiData.Type] = node;
            }
            return ai;
        }

        private static Node LoadBehaviorTree(ICombatState state, EnemyAIData data)
        {
            switch (data.Type)
            {
                default:
                case AIType.Easy:
                    return BuildEasyAIDecisionTree(state, data);
            }
        }

        private static Node BuildEasyAIDecisionTree(ICombatState state, EnemyAIData data)
        {
            var attackNode = new EasyAttackNode(state);
            var magicNode = new MpAttackNode(state);
            Func<Actor, bool> condition = (actor) => actor.Stats.Get(Stat.MP) > 0 && actor.HasMpMove();
            var actionDecisionNode = new BinaryDecisionNode(0.5f, magicNode, attackNode, condition);
            var healNode = new HealNode();
            var healthCheckNode = new HealthCheckNode(data.HpThreshold);
            var healthCondition = new ConditionNode(healthCheckNode, healNode);
            var baseNode = new SelectorNode(new List<Node> { healthCondition, actionDecisionNode });
            return new AITurnNode(baseNode);
        }
    }
}
