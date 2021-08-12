using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_GameData;
using RPG_GameState;

public class Village : Map
{
    [SerializeField] Sprite VillianSprite;
    [SerializeField] Actor FireSpirit;
    [SerializeField] GameObject Grandmom;
    [SerializeField] GameObject Doll;

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
        if (gameLogic.Settings.SkipCutscenes)
            gameState.CompleteEventInArea(Area, "opening_cutscene");
        else
            PlayOpeningScene(gameState, gameLogic.Stack);
    }

    private void PlayOpeningScene(GameState gameState, StateStack stack)
    {
        var hero = gameState.World.Party.GetActor(0).GetComponent<Character>();
        ServiceManager.Get<RPG_Audio.AudioManager>().SetOverallVolume(0.5f);
        var villainAsset = ServiceManager.Get<AssetManager>().Load<Character>(Constants.HERO_PREFAB);
        var fireAsset = ServiceManager.Get<AssetManager>().Load<Actor>(Constants.FIRE_SPIRIT_PREFAB);
        var villian = Instantiate(villainAsset);
        villian.Init(Constants.ENEMY_STATES);
        FireSpirit = Instantiate(fireAsset);
        FireSpirit.gameObject.SafeSetActive(false);
        FireSpirit.transform.position = Grandmom.transform.position;
        FireSpirit.Init(ServiceManager.Get<GameData>().PartyDefs["mage"]);
        FireSpirit.GetComponent<Character>().Init(Constants.PARTY_STATES);
        villian.transform.SetParent(NPCParent, false);
        villian.GetComponent<SpriteRenderer>().sprite = VillianSprite;
        var events = new List<IStoryboardEvent>
        {
            
            StoryboardEventFunctions.BlackScreen("black", 0.1f),
            StoryboardEventFunctions.MoveCharacterToPosition(hero.transform, new Vector2(65, 8)),
            StoryboardEventFunctions.FadeScreenOut("black", 1.0f),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_1_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_2_TEXT"),
            StoryboardEventFunctions.MoveCharacterToPosition(villian.transform, new Vector2(66, 6)),
            StoryboardEventFunctions.Wait(0.5f),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_3_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_4_TEXT"),
            StoryboardEventFunctions.MoveCharacter(hero, new List<Direction> { Direction.West, Direction.South }),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_5_TEXT"),
            StoryboardEventFunctions.MoveCharacterToPosition(hero.transform, new Vector2(65, 10)),
            StoryboardEventFunctions.MoveCharacter(villian, 
                new List<Direction> { Direction.North, Direction.East, Direction.East }),
            StoryboardEventFunctions.SetCharacterDirection(villian, Direction.North),
            StoryboardEventFunctions.Wait(0.3f),
            StoryboardEventFunctions.Function(() => Doll.gameObject.SafeSetActive(false)),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_6_TEXT"),
            StoryboardEventFunctions.Function(() => 
                {
                    Grandmom.SafeSetActive(false);
                    FireSpirit.gameObject.SafeSetActive(true);
                }),
            StoryboardEventFunctions.MoveCharacter(villian, 
                new List<Direction> { Direction.West, Direction.West, Direction.South, Direction.South }),
            StoryboardEventFunctions.Function(() => villian.gameObject.SafeSetActive(false)),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_7_TEXT"),
            StoryboardEventFunctions.Say("ID_OPENING_CUTSCENE_8_TEXT"),
            StoryboardEventFunctions.Function(() => gameState.World.Party.Add(FireSpirit)),
            StoryboardEventFunctions.Function(() => gameState.CompleteEventInArea(Area, "opening_cutscene")),
            StoryboardEventFunctions.Function(() => gameState.World.UnlockInput())
        };
        gameState.World.LockInput();
        var storyboard = new Storyboard(stack, events);
        stack.Push(storyboard);
    }
}
