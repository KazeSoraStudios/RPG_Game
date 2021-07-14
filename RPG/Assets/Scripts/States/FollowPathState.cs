using UnityEngine;
using RPG_Character;

public class FollowPathState : CharacterState
{

    public FollowPathState(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        if (!Character.HasValidPath())
        {
            Character.ResetDefaultState();
            Character.Controller.Change(Character.defaultState, null);
        }
        var direction = Character.GetNextPathDirection();
        switch (direction)
        {
            case Direction.North:
                Character.Controller.Change(Constants.UNIT_MOVE_STATE, new MoveParams(Vector2.up));
                break;
            case Direction.South:
                Character.Controller.Change(Constants.UNIT_MOVE_STATE, new MoveParams(Vector2.down));
                break;
            case Direction.East:
                Character.Controller.Change(Constants.UNIT_MOVE_STATE, new MoveParams(Vector2.left));
                break;
            case Direction.West:
                Character.Controller.Change(Constants.UNIT_MOVE_STATE, new MoveParams(Vector2.right));
                break;
            case Direction.NoWhere:
            default:
                Character.ResetDefaultState();
                Character.Controller.Change(Character.defaultState, null);
                break;
        }
    }

    public override void Exit()
    {
        Character.IncrementPathIndex();
    }

    public override string GetName()
    {
        return "FollowPathState";
    }
}
