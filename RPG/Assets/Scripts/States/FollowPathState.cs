using UnityEngine;
using RPG_Character;

public class FollowPathState : CharacterState
{

    public FollowPathState(Map map, Character character)
        : base(map, character) { }

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
            case Character.Direction.North:
                Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(Vector2.up));
                break;
            case Character.Direction.South:
                Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(Vector2.down));
                break;
            case Character.Direction.East:
                Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(Vector2.left));
                break;
            case Character.Direction.West:
                Character.Controller.Change(Constants.MOVE_STATE, new MoveParams(Vector2.right));
                break;
            case Character.Direction.NoWhere:
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
