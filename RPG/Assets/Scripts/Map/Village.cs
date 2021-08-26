using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_GameData;
using RPG_GameState;

public class Village : Map
{
    [SerializeField] Sprite VillianSprite;
    [SerializeField] GameObject Grandmom;
    [SerializeField] GameObject Doll;

    private Character villian;
    private Actor FireSpirit;

    private void Awake()
    {
        GameEventsManager.Register(GameEventConstants.START_BATTLE4, PlayBattle4Cutscene);
    }

    private void OnDestroy()
    {
        GameEventsManager.Unregister(GameEventConstants.START_BATTLE4, PlayBattle4Cutscene);
    }

    public override void Init()
    {
        base.Init();
        if (Area == null)
        {
            LogManager.LogError("Village Map area is null, cannot run Village Events.");
            return;
        }
        LoadEntities();
        var gameLogic = ServiceManager.Get<GameLogic>();
        var gameState = gameLogic.GameState;
        if (gameState.Areas.ContainsKey(Area.Id) && gameState.Areas[Area.Id].Events["opening_cutscene"])
            return;
        if (gameLogic.Settings.SkipCutscenes)
        {
            gameState.CompleteEventInArea(Area, "opening_cutscene");
            gameState.World.Party.Add(FireSpirit);
        }
        else
            PlayOpeningScene(gameState, gameLogic.Stack);
    }

    private void PlayOpeningScene(GameState gameState, StateStack stack)
    {
        var hero = gameState.World.Party.GetActor(0).GetComponent<Character>();
        ServiceManager.Get<RPG_Audio.AudioManager>().SetOverallVolume(0.5f);

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

    private void PlayBattle4Cutscene(object obj)
    {
        var party = ServiceManager.Get<Party>();
        party.PrepareForCombat();
        var gameLogic = ServiceManager.Get<GameLogic>();
        var stack = gameLogic.Stack;
        var battle = ServiceManager.Get<GameData>().Battles["battle4"];
        var enemies = LoadBattle4Entities();
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.Say(battle.BeforeText[0]),
            StoryboardEventFunctions.Say(battle.BeforeText[1]),
            StoryboardEventFunctions.MoveCharacters(new List<Character> { enemies[0], enemies[1], enemies[2] }, 
                new List<Direction> { Direction.South, Direction.South }),
            StoryboardEventFunctions.Say(battle.BeforeText[2]),
            StoryboardEventFunctions.Function(() =>
            {
                var config = new Actions.StartCombatConfig
                {
                    CanFlee = false,
                    Enemies = battle.Enemies,
                    Party = party.Members,
                    OnWin = () => PlayBattle4WinCutscene((enemies)),
                    Stack = stack
                };
                Actions.Combat(config);
            })
        };
        gameLogic.GameState.World.LockInput();
        var storyboard = new Storyboard(stack, events);
        stack.Push(storyboard);
    }

    private void PlayBattle4WinCutscene(List<Character> enemies)
    {
        var gameLogic = ServiceManager.Get<GameLogic>();
        var stack = gameLogic.Stack;
        var battle = ServiceManager.Get<GameData>().Battles["battle4"];
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.Say(battle.AfterText[0]),
            StoryboardEventFunctions.Say(battle.AfterText[1]),
            StoryboardEventFunctions.MoveCharacters(new List<Character> { enemies[0], enemies[1], enemies[2] }, 
                new List<Direction> { Direction.North, Direction.North }),
            StoryboardEventFunctions.Function(() =>
            {
                foreach (var enemy in enemies)
                    enemy.gameObject.SafeSetActive(false);
            }),
            StoryboardEventFunctions.Function(() => gameLogic.GameState.World.UnlockInput())
        };
        var storyboard = new Storyboard(stack, events);
        stack.Push(storyboard);
    }

    private void LoadEntities()
    {
        var villainAsset = ServiceManager.Get<AssetManager>().Load<Character>(Constants.HERO_PREFAB);
        villian = Instantiate(villainAsset, NPCParent);
        villian.Init(Constants.ENEMY_STATES);
        villian.GetComponent<SpriteRenderer>().sprite = VillianSprite;
        villian.GetComponent<Animator>().enabled = false;

        var fireAsset = ServiceManager.Get<AssetManager>().Load<Actor>(Constants.FIRE_SPIRIT_PREFAB);
        FireSpirit = Instantiate(fireAsset, NPCParent);
        FireSpirit.gameObject.SafeSetActive(false);
        FireSpirit.transform.position = Grandmom.transform.position;
        FireSpirit.Init(ServiceManager.Get<GameData>().PartyDefs["mage"]);
        FireSpirit.GetComponent<Character>().Init(Constants.PARTY_STATES);
    }
    
    private List<Character> LoadBattle4Entities()
    {
        var characters = new List<Character>();
        var skeletonAsset = ServiceManager.Get<AssetManager>().Load<Character>(Constants.SKELETON_PREFAB);
        var skeleton = Instantiate(skeletonAsset, new Vector3(-3.5f, 14.0f, 0.0f), Quaternion.identity, NPCParent);
        skeleton.Init(Constants.ENEMY_STATES);
        skeleton.GetComponent<Animator>().enabled = false;
        characters.Add(skeleton);
        
        var fireSpiritAsset = ServiceManager.Get<AssetManager>().Load<Character>(Constants.FIRE_SPIRIT_PREFAB);
        var fireSpirit = Instantiate(fireSpiritAsset, new Vector3(-4.5f, 14.0f, 0.0f), Quaternion.identity, NPCParent);
        fireSpirit.Init(Constants.ENEMY_STATES);
        fireSpirit.GetComponent<Animator>().enabled = false;
        characters.Add(fireSpirit);
        
        var kappaAsset = ServiceManager.Get<AssetManager>().Load<Character>(Constants.KAPPA_PREFAB);
        var kappa = Instantiate(kappaAsset, new Vector3(-5.5f, 14.0f, 0.0f), Quaternion.identity, NPCParent);
        kappa.Init(Constants.ENEMY_STATES);
        kappa.GetComponent<Animator>().enabled = false;
        characters.Add(kappa);
        
        return characters;
    }
}
