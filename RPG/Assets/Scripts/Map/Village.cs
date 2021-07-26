using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_GameState;

public class Village : Map
{
    public override void Init()
    {
        base.Init();
        if (Area == null)
        {
            LogManager.LogError("Village Map area is null, cannot run Village Events.");
            return;
        }
        var gameLogic = ServiceManager.Get<GameLogic>();
        var gameState = gameLogic.GameState;
        if (gameState.Areas.ContainsKey(Area.Id) && gameState.Areas[Area.Id].Events["opening_cutscene"])
            return;
        PlayOpeningScene(gameState, gameLogic.Stack);
    }

    private void PlayOpeningScene(GameState gameState, StateStack stack)
    {
        var character = gameState.World.Party.GetActor(0).GetComponent<Character>();
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen("black", 0.1f),
            StoryboardEventFunctions.MoveHeroToPosition(this, new Vector2(65, 8)),
            StoryboardEventFunctions.FadeScreenOut("black", 1.0f),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_1_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_2_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_3_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_4_TEXT"),
            StoryboardEventFunctions.MoveCharacter(character, 
                new List<Direction> {Direction.West, Direction.South, Direction.South }),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_5_TEXT"),
            StoryboardEventFunctions.SetCharacterDirection(character, Direction.North),
            StoryboardEventFunctions.Wait(0.3f),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_6_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_7_TEXT"),
            StoryboardEventFunctions.SetCharacterDirection(character, Direction.South),
            StoryboardEventFunctions.Function(() => gameState.CompleteEventInArea(Area, "opening_cutscene")),
            StoryboardEventFunctions.Function(() => gameState.World.UnlockInput())
        };
        gameState.World.LockInput();
        var storyboard = new Storyboard(stack, events);
        stack.Push(storyboard);
    }
}
