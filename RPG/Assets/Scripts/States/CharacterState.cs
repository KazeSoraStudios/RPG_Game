using UnityEngine;
using RPG_Character;

public abstract class CharacterState : IState
{
    public Character Character;

    public CharacterState() { }

    public CharacterState(Character character)
    {
        Character = character;
    }

    public virtual bool Execute(float deltaTime) { return true; }
    public virtual void Enter(object stateParams) { }
    public virtual void Exit() { }
    public abstract string GetName();
}

public class NpcStandState : CharacterState
{
    private const string Name = "NPCStandState";
    public override string GetName() { return Name; }

}

public class PlanStrollState : CharacterState
{
    private const string Name = "PlanStrollState";

    private int FrameCount;
    private float CountDown;

    public PlanStrollState(Character character)
        : base(character)
    {
        CountDown = Random.Range(0, 3);
    }

    public override string GetName() { return Name; }

    public override void Enter(object stateParams)
    {
        CountDown = Random.Range(0, 3);
        Character.UpdateMovement(Vector2.zero);
    }

    public override bool Execute(float deltaTime)
    {
        CountDown -= deltaTime;
        if (CountDown <= 0)
        {
            var choice = Random.Range(1, 4);
            var direction = Vector2.zero;
            if (choice == 1)
                direction = Vector2.left;
            else if (choice == 2)
                direction = Vector2.right;
            else if (choice == 3)
               direction = Vector2.up;
            else if (choice == 4)
                direction = Vector2.down;
            Character.Controller.Change(Constants.UNIT_MOVE_STATE, new MoveParams(direction));
            CountDown = Random.Range(0, 3);
        }
        return true;
    }
}

public class WaitState : CharacterState
{
    private const string Name = "WaitState";

    public WaitState(Character character)
        : base(character) { }

    public override string GetName() { return Name; }

    public override void Enter(object stateParams)
    {
        Character.UpdateMovement(Vector2.zero);
    }

    public override bool Execute(float deltaTime)
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement != Vector2.zero)
        {
            if (movement.x != 0.0f)
                movement.y = 0.0f;
            else
                movement.x = 0.0f;
            Character.UpdateMovement(movement);
        }
        else
        {
            Character.UpdateMovement(movement);
            Character.Controller.Change(Character.defaultState);
        }
        return true;
    }
}

public class UnitMoveState : CharacterState
{
    private const string Name = "MoveState";
    private Vector2 currentPosition = Vector2.zero;
    private Vector2 targetPosition = Vector2.zero;

    public UnitMoveState(Character character)
        : base(character) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Enter(object stateParams)
    {
        if (!(stateParams is MoveParams moveParams))
        {
            Character.Controller.Change(Character.defaultState);
            return;
        }
        BeforeMove(moveParams.MovePosition);
    }

    public override void Exit()
    {
        var position = Character.transform.position;
    }

    public float time = 0.0f;
    public override bool Execute(float deltaTime)
    {
        time += deltaTime;
        currentPosition = Vector2.Lerp(currentPosition, targetPosition, time / 0.75f);
        Character.transform.position = currentPosition;
        var distance = Vector2.Distance(currentPosition, targetPosition);
        if (distance <= 0.02f)
        {
            Character.transform.position = targetPosition;
            var position = Character.transform.position;
            Character.Controller.Change(Character.defaultState);
        }
        return true;
    }

    private void BeforeMove(Vector2 movement)
    {
        time = .0f;
        currentPosition = Character.transform.position;
        targetPosition = currentPosition + movement;
        Character.UpdateStoryboardMovement(movement);
    }
}

public class MoveState : CharacterState
{
    private const string Name = "MoveState";

    public MoveState(Character character)
        : base(character) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Enter(object stateParams)
    {
        if (!(stateParams is MoveParams moveParams))
        {
            Character.Controller.Change(Character.defaultState);
            return;
        }
        int x = (int)Character.transform.position.x + (int)moveParams.MovePosition.x;
        int y = (int)Character.transform.position.y + (int)moveParams.MovePosition.y;
        Character.UpdateMovement(moveParams.MovePosition);
    }

    public override void Exit()
    {
        
    }

    public override bool Execute(float deltaTime)
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement != Vector2.zero)
        {
            if (movement.x != 0.0f)
                movement.y = 0.0f;
            else
                movement.x = 0.0f;
            Character.UpdateMovement(movement);
        }
        else
        {
            Character.Controller.Change(Character.defaultState);
        }
        return true;
    }
}