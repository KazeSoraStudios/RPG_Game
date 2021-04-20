using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public abstract class CombatState : IState
    {
        protected string previousState;
        protected Character character;

        public CombatState(Character character)
        {
            this.character = character;
        }

        public virtual bool Execute(float deltaTime) { return true; }
        public virtual void Enter(object stateParams) { }
        public virtual void Exit() { }
        public bool IsPartyMember(Actor actor) { return false; }
        public virtual bool IsFinished() { return true; }
        public abstract string GetName(); // TODO change to int
    }

    public class CombatStateParams : StateParams
    {
        public int Direction;
        public float MoveTime;
        public Vector2 MovePosition;
        public Color flashColor;
        public string State;
    }

    public class CSStandBy : CombatState
    {
        public CSStandBy(Character character)
            : base(character) { }

        public override void Enter(object stateParams)
        {
            character.SetCombatStandBy();
        }

        public override string GetName()
        {
            return "CombatStandBy";
        }
    }

    public class CSHurt : CombatState
    {
        public CSHurt(Character character)
            : base(character) { }

        public override void Enter(object stateParams)
        {
            if (stateParams is CombatStateParams cs && cs != null)
            {
                previousState = cs.State;
                character.PlayAnimation(Constants.HURT_ANIMATION);
            }
        }

        public override bool Execute(float deltaTime)
        {
            if (!character.IsAnimationFinished(Constants.HURT_ANIMATION))
                character.Controller.Change(previousState);
            return true;
        }

        public override string GetName()
        {
            return "CombatStateHurt";
        }
    }

    public class CSMove : CombatState
    {
        private Direction direction = Direction.West;
        private float moveTime = 0.6f;
        private float t = 0.0f;
        private Vector2 targetPosition = Vector2.zero;

        public CSMove(Character character)
            : base(character) { }

        public override void Enter(object stateParams)
        {
            if (!(stateParams is CombatStateParams combatParams))
            {
                character.Controller.Change(character.defaultState);
                return;
            }
            targetPosition = (Vector2)character.transform.position + combatParams.MovePosition;
            direction = combatParams.Direction == 0 ? Direction.West : Direction.East;
            if (combatParams.MoveTime > 0.0f)
                moveTime = combatParams.MoveTime;
            character.CombatMovement(combatParams.MovePosition, direction);
            t = 0.0f;

        }

        public override bool Execute(float deltaTime)
        {
            t += deltaTime;
            var distance = Vector2.Distance(character.transform.position, targetPosition);
            if (distance <= 0.1f)
            {
                character.transform.position = targetPosition;
                character.UpdateMovement(Vector2.zero);
                t = moveTime;
            }
            return true;
        }

        public override bool IsFinished()
        {
            return t >= moveTime;
        }

        public override string GetName()
        {
            return "CombatStateMove";
        }
    }

    public class CSRunAnimation : CombatState
    {
        public class Config : StateParams
        {
            public string Animation;
        }

        private string animationName;

        public CSRunAnimation(Character character)
            : base(character) { }

        public override void Enter(object stateParams)
        {
            if (!(stateParams is Config config))
            {
                character.Controller.Change(character.defaultState);
                return;
            }
            animationName = config.Animation;
            character.PlayAnimation(config.Animation);
        }

        public override string GetName()
        {
            return "CombatStateRunAnimation";
        }

        public override bool IsFinished()
        {
            return character.IsAnimationFinished(animationName);
        }
    }

    public class CSEnemyHurt : CombatState
    {
        private int knockback = 3;
        private float moveTime = 0.2f;
        private float t = 0.0f;
        private Vector2 originalPosition = Vector2.zero;
        private Color flashColor = Color.yellow;
        private SpriteRenderer spriteRenderer;

        public CSEnemyHurt(Character character)
            : base(character) { }

        public override void Enter(object stateParams)
        {
            if (!(stateParams is CombatStateParams cs) || cs == null)
            {
                return;
            }

            previousState = cs.State;
            character.PlayAnimation(Constants.HURT_ANIMATION);
            moveTime = cs.MoveTime;
            originalPosition = character.transform.position;
            character.transform.position = character.transform.position + (Vector3)cs.MovePosition;
            flashColor = cs.flashColor;
            spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.color = flashColor;
        }

        public override bool Execute(float deltaTime)
        {
            t += deltaTime;
            var distance = Vector2.Distance(character.transform.position, originalPosition);
            if (distance <= 0.1f)
            {
                character.transform.position = originalPosition;
                character.Controller.SetCurrentState(previousState);
            }
            else
            {
                var characterPosition = character.transform.position;
                var percent = t / moveTime;
                var position = Vector2.Lerp(characterPosition, originalPosition, percent);
                var color = Color.Lerp(flashColor, Color.white, percent);
                character.UpdateMovement(position);
            }
            return true;
        }

        public override void Exit()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }

        public override string GetName()
        {
            return "CombatStateEnemyHurt";
        }
    }

    public class CSEnemyDie : CombatState
    {
        private float enemyDeathTime = 1.0f;
        private float time = 0.0f;
        private SpriteRenderer renderer;
        public CSEnemyDie(Character character)
        : base(character) { }

        public override void Enter(object stateParams)
        {
            character.PlayAnimation(Constants.DEATH_ANIMATION);
            renderer = character.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                LogManager.LogError($"Cannot find sprite renderer on character: {character.name}. Changing to default state");
                character.Controller.Change(character.defaultState, null);
            }
        }

        public override bool Execute(float deltaTime)
        {
            time += deltaTime;
            var color = renderer.color;
            color.a = time / enemyDeathTime;
            renderer.color = color;
            return true;
        }

        public override bool IsFinished()
        {
            return time >= enemyDeathTime;
        }

        public override string GetName()
        {
            return "CombatStateEnemyDie";
        }
    }
}