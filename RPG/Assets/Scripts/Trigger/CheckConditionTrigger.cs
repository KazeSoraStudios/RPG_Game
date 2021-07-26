using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_GameData;

public class CheckConditionTrigger : MonoBehaviour
{
    [SerializeField] List<string> Conditions;
    
    private int currentCondition = 0;
    private Area Area;

    public void Init(Area area)
    {
        this.Area = area;
        for (int i = Conditions.Count - 1; i >= 0; i--)
        {
            var condition = Conditions[i];
            if (!Area.Events.ContainsKey(condition))
            {
                LogManager.LogError($"Condition [{Conditions}] is not in Area {Area.Id}, removing from Conditions");
                Conditions.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.tag.Equals("Player"))
            return;
        currentCondition = AllConditionsMet();
        if (currentCondition == -1)
        {
            gameObject.SafeSetActive(false);
            LogManager.LogDebug($"All Conditions met for {name}, turning off.");
            return;
        }
        var gameState = ServiceManager.Get<GameLogic>().GameState;
        gameState.World.LockInput();
        var character = gameState.World.Party.Members[0].GetComponent<Character>();
        var locId = $"ID_{Conditions[currentCondition]}_CONDITION_TEXT".ToUpper();
        var events = new List<IStoryboardEvent>()
        {
            StoryboardEventFunctions.Say(locId),
            StoryboardEventFunctions.MoveCharacter(character, new List<Direction> { GetOppositeDirection(character.direction) }),
            StoryboardEventFunctions.Function(() => gameState.World.UnlockInput())
        };
        var stack = ServiceManager.Get<GameLogic>().Stack;
        var storyboard = new Storyboard(stack, events);
        stack.Push(storyboard);
    }

    private int AllConditionsMet()
    {
        for (int i = 0; i < Conditions.Count; i++)
            if (!Area.Events[Conditions[i]])
                return i;
        return -1;
    }

    private Direction GetOppositeDirection(Direction current)
    {
        switch (current)
        {
            case Direction.North:
                return Direction.South;
            case Direction.South:
                return Direction.North;
            case Direction.East:
                return Direction.West;
            case Direction.West:
                return Direction.East;
            default:
            return Direction.North;
        }
    }
}
