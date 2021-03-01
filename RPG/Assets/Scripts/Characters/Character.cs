﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Character : MonoBehaviour
{
    [SerializeField] public int PathIndex = 0;
    [SerializeField] public Animator animator;
    [SerializeField] public Map Map;
    [SerializeField] public StateMachine Controller { get; private set; }
    [SerializeField] public string defaultState = Constants.WAIT_STATE;
    [SerializeField] public string previousDefaultState = Constants.WAIT_STATE;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] public Entity Entity;
    [SerializeField] public List<Direction> PathToMove = new List<Direction>();

    public enum Direction { North,South,East,West, NoWhere };
    public Direction direction;
    
    // Start is called before the first frame update
    public void Init(Map map)
    {
        Map = map;
        var states = new Dictionary<string, IState>();
        states.Add(defaultState, new WaitState(Map, this));
        states.Add(Constants.MOVE_STATE, new MoveState(Map, this));
        states.Add(Constants.FOLLOW_PATH_STATE, new FollowPathState(Map, this));

        Controller = new StateMachine(states, states[defaultState]);
        Entity = GetComponent<Entity>();
        //Controller.States = states;
        //Controller.CurrentState = states[States.Wait];
    }
    public Vector2 force;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var p = new List<Direction> { Direction.East, Direction.East, Direction.East, Direction.North,
            Direction.West,Direction.West,Direction.West,Direction.South, Direction.South};
            FollowPath(p);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Entity.AddKnockback(force);
        }


        if (Input.GetKeyDown(KeyCode.C))
        {
            var p = transform.position + (Vector3)force;
            transform.position = p;
        }

        Vector3 facing = Vector3.down;
        if (direction == Direction.East)
            facing = Vector3.left;
        else if (direction == Direction.West)
            facing = Vector3.right;
        else if (direction == Direction.North)
            facing = Vector3.up;
        else if (direction == Direction.South)
            facing = Vector3.down;

        //Debug.DrawRay(transform.position, 0.5f, Color.red);

        var Grid = GameObject.Find("Collision").GetComponent<TilemapRenderer>();
        var x = Grid.sortingOrder = 2;
    }

    public bool CanMove(Vector2 targetPosition)
    {
        var p = (Vector2)transform.position + targetPosition;
        var collision = Physics2D.OverlapCircle(p, 0.2f, collisionLayer);
        return !collision;
    }

    public void UpdateMovement(Vector2 movement)
    {
        Entity.UpdateMovement(movement, Map);
        UpdateAnimation(movement);
    }

    public void UpdateAnimation(Vector2 movement)
    {
        UpdatePlayerDirection(movement);
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
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
        PathIndex = 1;
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
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
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
}