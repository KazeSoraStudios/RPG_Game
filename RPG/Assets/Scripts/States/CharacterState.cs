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
    private float FrameResetSpeed = 0.05f;

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

            if (Character.CanMove(direction))
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
            if (!Character.CanMove(movement))
            {
                Character.UpdateAnimation(movement);
                return true;
            }
            //Character.Controller.SetParams(movement);
            Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(movement));
        }
        else
        {
            Character.UpdateAnimation(Vector2.zero);
        }
        return true;
    }
}

public class UnitMoveState : CharacterState
{
    private const string Name = "MoveState";
    private Trigger Trigger;
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
        Trigger = ServiceManager.Get<TriggerManager>().GetTrigger((int)position.x, (int)position.y);
        Trigger.OnEnter(new TriggerParams((int)position.x, (int)position.y, Character));
        Trigger = null;
    }

    public override bool Execute(float deltaTime)
    {
        var distance = Vector2.Distance(Character.transform.position, targetPosition);
        if (distance <= 0.05f)
        {
            Character.transform.position = targetPosition;
            var position = Character.transform.position;
            Trigger = ServiceManager.Get<TriggerManager>().GetTrigger((int)position.x, (int)position.y);
            Trigger.OnEnter(new TriggerParams((int)position.x, (int)position.y, Character));
            var movement = GetNewMovement();
            if (movement == Vector2.zero)
                Character.Controller.Change(Character.defaultState);
            else
                BeforeMove(movement);
        }
        return true;
    }

    private void BeforeMove(Vector2 movement)
    {
        var triggerPosition = Character.transform.position;
        Trigger = ServiceManager.Get<TriggerManager>().GetTrigger((int)triggerPosition.x, (int)triggerPosition.y);
        Trigger.OnExit(new TriggerParams((int)triggerPosition.x, (int)triggerPosition.y, Character));
        targetPosition = (Vector2)triggerPosition + movement;
        // if (Map.TryEncounter(new Vector3Int((int)targetPosition.x, (int)targetPosition.y, 0)))
        // {

        // }
        Character.UpdateMovement(movement);
    }

    private Vector2 GetNewMovement()
    {
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.x != 0.0f)
            movement.y = 0.0f;
        else
            movement.x = 0.0f;
        return Character.CanMove(movement) ? movement : Vector2.zero;
    }
}

public class MoveState : CharacterState
{
    private const string Name = "MoveState";
    private Trigger Trigger;

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
        var map = Character.Map;
        if (map.TryEncounter(new Vector3Int(x, y, 0)))
        {

        }
        Character.UpdateMovement(moveParams.MovePosition);
    }

    public override void Exit()
    {
        var position = Character.transform.position;
        // Trigger = ServiceManager.Get<TriggerManager>().GetTrigger((int)position.x, (int)position.y);
        // Trigger.OnEnter(new TriggerParams((int)position.x, (int)position.y, Character));
        Trigger = null;
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