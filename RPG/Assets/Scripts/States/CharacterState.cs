using UnityEngine;

public abstract class CharacterState : IState
{
    protected Map Map;
    protected Character Character;

    public CharacterState() { }

    public CharacterState(Map map, Character character)
    {
        Map = map;
        Character = character;
    }

    public virtual bool Execute(float deltaTime) { return true; }
    public virtual void Enter(object stateParams) { }
    public virtual void Exit() { }
    public abstract string GetName(); // TODO change to int
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

    public PlanStrollState(Map map, Character character)
        : base(map, character)
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
                Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(direction));
            CountDown = Random.Range(0, 3);
        }
        return true;
    }
}

public class WaitState : CharacterState
{
    private const string Name = "WaitState";

    public WaitState(Map map, Character character)
        : base(map, character) { }

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

public class MoveState : CharacterState
{
    private const string Name = "MoveState";
    private Trigger Trigger;
    private Vector2 targetPosition = Vector2.zero;

    public MoveState(Map map, Character character)
        : base(map, character) { }

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
        var triggerPosition = Character.transform.position;
        Trigger = Map.GetTrigger((int)triggerPosition.x, (int)triggerPosition.y);
        Trigger.OnExit(new TriggerParams((int)triggerPosition.x, (int)triggerPosition.y, Character));
        //else
        //    Map.TryEncounter(x,y,layer)
        targetPosition = (Vector2)triggerPosition + moveParams.MovePosition;
        Character.UpdateMovement(moveParams.MovePosition);
    }

    public override void Exit()
    {
        var position = Character.transform.position;
        Trigger = Map.GetTrigger((int)position.x, (int)position.y);
        Trigger.OnEnter(new TriggerParams((int)position.x, (int)position.y, Character));
        Trigger = null;
    }

    public override bool Execute(float deltaTime)
    {
        var distance = Vector2.Distance(Character.transform.position, targetPosition);
        if (distance <= 0.1f)
        {
            Character.transform.position = targetPosition;
            var position = Character.transform.position;
            Trigger = Map.GetTrigger((int)position.x, (int)position.y);
            Trigger.OnEnter(new TriggerParams((int)position.x, (int)position.y, Character));
            Character.Controller.Change(Character.defaultState);
        }
        return true;
    }
}