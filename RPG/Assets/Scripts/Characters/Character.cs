using System.Collections.Generic;
using UnityEngine;
using RPG_Combat;

namespace RPG_Character
{
    public enum CharacterType { Party, Enemy, NPC };
    public enum Direction { North, South, East, West, NoWhere };
    public class Character : MonoBehaviour
    {
        [SerializeField] public int PathIndex = 0;
        [SerializeField] public Animator animator;
        [SerializeField] public StateMachine Controller { get; private set; }
        [SerializeField] public string defaultState = Constants.WAIT_STATE;
        [SerializeField] public string previousDefaultState = Constants.WAIT_STATE;
        [SerializeField] public LayerMask collisionLayer;
        [SerializeField] public Entity Entity;
        [SerializeField] public List<Direction> PathToMove = new List<Direction>();

        public Direction direction;

        private bool hasSpeedBeforeText = false;
        private Direction directionBeforeCombat = Direction.South;
        private Vector2 movementBeforeTextbox = Vector2.zero;
        private Vector2 locationBeforeCombat = Vector2.zero;

        public void Init(List<string> states, string defaultState = Constants.STAND_STATE)
        {
            this.defaultState = defaultState;
            BuildStateMachine(states);
            Entity = GetComponent<Entity>();
        }

        public void CombatMovement(Vector2 movement, Direction direction)
        {
            Entity.UpdateMovement(movement);
            var directionMovement = direction == Direction.South ? Vector2.down : Vector2.up;
            UpdateAnimation(directionMovement);
        }

        public void UpdateMovement(Vector2 movement)
        {
            Entity.UpdateMovement(movement);
            UpdateAnimation(movement);
        }

        public void UpdateStoryboardMovement(Vector2 movement)
        {
            UpdateAnimation(movement);
        }

        public void UpdateAnimation(Vector2 movement)
        {
            UpdatePlayerDirection(movement);
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }

        public void UpdateDirection(Direction direction)
        {
            this.direction = direction;
            animator.SetInteger("Direction", (int)direction);
            switch (direction)
            {
                case Direction.North:
                    animator.Play("IdleUp");
                    break;
                case Direction.South:
                    animator.Play("IdleDown");
                    break;
                case Direction.East:
                    animator.Play("IdleLeft");
                    break;
                case Direction.West:
                    animator.Play("IdleLeft");
                    break;
                default:
                break;
            }
        }

        void UpdatePlayerDirection(Vector2 movement)
        {
            if (movement == Vector2.zero)
                return;
            if (movement.x != 0)
            {
                if (movement.x > 0)
                    direction = Direction.West;
                else
                    direction = Direction.East;
            }
            else
            {
                if (movement.y > 0)
                    direction = Direction.North;
                else
                    direction = Direction.South;
            }
            animator.SetInteger("Direction", (int)direction);
        }

        public Vector2 GetFacingTilePosition()
        {
            var position = (Vector2)transform.position;
            switch (direction)
            {
                case Direction.North:
                    return position + Vector2.up;
                case Direction.South:
                    return position + Vector2.down;
                case Direction.East:
                    return position + Vector2.left;
                case Direction.West:
                    return position + Vector2.right;
                default:
                    return position;
            }
        }

        public bool HasValidPath()
        {
            return PathIndex != -1 && PathToMove.Count > 0 && PathIndex < PathToMove.Count;
        }

        public void FollowPath(List<Direction> path)
        {
            PathIndex = 0;
            PathToMove.Clear();
            PathToMove = path;
            previousDefaultState = defaultState;
            defaultState = Constants.FOLLOW_PATH_STATE;
            Controller.Change(defaultState, null);
        }

        public Direction GetNextPathDirection()
        {
            if (PathIndex < 0 || PathIndex >= PathToMove.Count)
                return Direction.NoWhere;
            return PathToMove[PathIndex];

        }

        public void IncrementPathIndex()
        {
            PathIndex++;
        }

        public void ResetDefaultState()
        {
            defaultState = previousDefaultState;
        }

        public void SetCombatStandBy()
        {
            animator.SetBool("combat", true);
        }

        public void PlayAnimation(string animation)
        {
            animator.Play(animation);
        }

        public bool IsAnimationFinished(string animation)
        {
            return !animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
        }

        public bool IsAnimationPlaying(string animation)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
        }

        public bool IsHero()
        {
            // TODO implement
            return false;
        }

        public void PrepareForTextState()
        {
            movementBeforeTextbox.x = animator.GetFloat("Horizontal");
            movementBeforeTextbox.x = animator.GetFloat("Vertical");
            hasSpeedBeforeText = animator.GetFloat("Speed") != 0;
            Entity.PrepareForTextbox();
            ResetAnimator();
        }

        public void ReturnFromTextState()
        {
            animator.SetFloat("Horizontal", movementBeforeTextbox.x);
            animator.SetFloat("Vertical", movementBeforeTextbox.y);
            animator.SetFloat("Speed", hasSpeedBeforeText ? 1 : 0);
            Entity.ReturnFromTextbox();
        }

        public void PrepareForCombat()
        {
            locationBeforeCombat = transform.position;
            directionBeforeCombat = direction;
            ResetAnimator();
            Controller.Change(Constants.EMPTY_STATE);
            Entity.UpdateMovement(Vector2.zero);
        }

        public void ReturnFromCombat()
        {
            ResetAnimator();
            Controller.Change(defaultState);
            transform.position = locationBeforeCombat;
            animator.SetInteger("Direction", (int)directionBeforeCombat);
        }

        public void PrepareForTeleport()
        {
            ResetAnimator();
            Entity.UpdateMovement(Vector2.zero);
            Controller.Change(defaultState);
        }

        public void PrepareForSceneChange()
        {
            ResetAnimator();
            Entity.UpdateMovement(Vector2.zero);
            Controller.Change(defaultState);
        }

        private void ResetAnimator()
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Speed", 0);
        }

        private void BuildStateMachine(List<string> characterStates)
        {
            var states = new Dictionary<string, IState>();
            foreach (var state in characterStates)
                states.Add(state, BuildState(state));
            if (!states.ContainsKey(defaultState))
            {
                LogManager.LogError($"CharacterStates for Character [{name}] does not contain DefaultState [{defaultState}]");
                return;
            }
            Controller = new StateMachine(states, name, states[defaultState]);
        }

        private IState BuildState(string state)
        {
            switch (state)
            {
                case Constants.WAIT_STATE:
                    return new WaitState(this);
                case Constants.MOVE_STATE:
                    return new MoveState(this);
                case Constants.UNIT_MOVE_STATE:
                    return new UnitMoveState(this);
                case Constants.COMBAT_MOVE_STATE:
                    return new CSMove(this);
                case Constants.STROLL_STATE:
                    return new PlanStrollState(this);
                case Constants.STAND_STATE:
                    return new NpcStandState();
                case Constants.FOLLOW_PATH_STATE:
                    return new FollowPathState(this);
                case Constants.HURT_STATE:
                    return new CSHurt(this);
                // Constants.HURT_ENEMY_STATE: // TODO fix
                  //  return new CSEnemyHurt(this);
                case Constants.ENEMY_DIE_STATE:
                    return new CSEnemyDie(this);
                case Constants.USE_STATE:
                    return new CSEnemyDie(this);
                case Constants.RUN_ANIMATION_STATE:
                    return new CSRunAnimation(this);
                default:
                    LogManager.LogError($"Unkown CharacterState {state}, cannot build state.");
                    return null;
            }
        }
    }
}