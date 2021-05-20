using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RPG_UI;
using RPG_Combat;
using RPG_Character;
using RPG_GameData;
using RPG_GameState;
using System.Collections.Generic;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    [SerializeField] LogLevel LogLevel;
    [SerializeField] public GameState GameState;
    [SerializeField] UIController UIController;
    [SerializeField] GameDataDownloader GameDataDownloader;
    [SerializeField] public Image ScreenImage; //TODO move to UIController

    public StateStack Stack;

    private TriggerManager triggerManager;
    private LocalizationManager localizationManager;

    private void Awake()
    {
        ServiceManager.Register(this);
        LogManager.SetLogLevel(LogLevel);
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(Camera.main);
        DontDestroyOnLoad(GameObject.Find("UICanvas"));

        triggerManager = new TriggerManager();
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    private void Start()
    {
        Stack = new StateStack();
        UIController.InitUI();
        var gameManager = ServiceManager.Get<GameStateManager>();
        gameManager.LoadSavedGames();
        if (gameManager.GetNumberOfSaves() > 0)
            gameManager.LoadGameStateData(0);
        SetUpNewGame();
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Village", LoadSceneMode.Single);
        StartCoroutine(LoadVillage());
        //LoadMap();
    }

    IEnumerator LoadVillage()
    {
        yield return new WaitForSeconds(0.1f);
        var map = GameObject.Find("VillageMap").GetComponent<Map>();
        var exploreState = new ExploreState();
        exploreState.Init(map, Stack, Vector2.zero);
        Stack.Push(exploreState);
        yield return null;
    }

    public void OnMapLoaded(Map map)
    {
        //var exploreState = new ExploreState();
        //exploreState.Init(map, Stack, Vector2.zero);
        //Stack.Push(exploreState);
    }

    public void LoadGame()
    {
        
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    var events = new List<IStoryboardEvent>
        //    {
        //        StoryboardEventFunctions.BlackScreen(),
        //        StoryboardEventFunctions.FadeScreenOut(),
        //        StoryboardEventFunctions.Wait(2.0f),
        //        //StoryboardEventFunctions.FadeScreenIn(),
        //        StoryboardEventFunctions.HandOffToExploreState()
        //    };
        //    var storyboard = new Storyboard(Stack, events, true);
        //    Stack.Push(storyboard);
        //    LogManager.LogInfo("Pushed storyboard");
        //}

        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    Stack.PushTextbox("this is a sample text box", Constants.PORTRAIT_PATH + "mage_portrait");
        //}

        #if UNITY_EDITOR
        LogManager.SetLogLevel(LogLevel);
        #endif

        if (Input.GetKeyDown(KeyCode.G))
        {
            var events = new List<IStoryboardEvent>();
            events.Add(StoryboardEventFunctions.BlackScreen());
            events.Add(StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f));
            events.Add(new StoryboardFunctionEvent
                    {
                        Function = (_) =>
                        {
                            var loaded = false;
                            SceneManager.sceneLoaded += (x,y) => loaded = true;
                            SceneManager.LoadScene("Forest", LoadSceneMode.Additive);
                            return new BlockUntilEvent
                            {
                                UntilFunction = () =>
                                {
                                    return loaded == true;
                                }
                            };
                        }
                    });
            events.Add(StoryboardEventFunctions.Wait(1.0f));
            events.Add(StoryboardEventFunctions.Function(() =>
            {
                var forest = SceneManager.GetSceneByName("Forest");
                SceneManager.SetActiveScene(forest);
                SceneManager.UnloadSceneAsync("Village");
            }));
            //StoryboardEventFunctions.Wait(2.0f),
            events.Add(StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f));
            events.Add(StoryboardEventFunctions.Function(() =>StoryboardEventFunctions.ReplaceExploreStateMap(Constants.HANDIN_STATE, GameObject.Find("ForestMap").GetComponent<Map>())));
            events.Add(StoryboardEventFunctions.HandOffToExploreState());
            var storyboard = new Storyboard(Stack, events, true);
            Stack.Push(storyboard);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            var parent = UIController.CombatLayer;
            var obj = ServiceManager.Get<AssetManager>().Load<CombatGameState>(Constants.COMBAT_PREFAB_PATH);
            var combat = Instantiate(obj, Vector3.zero, Quaternion.identity);
            combat.transform.SetParent(parent, false);
            var npc = GameObject.Find("TestNPC(Clone)").GetComponent<RPG_Character.Actor>();
            npc.Stats.SetStat(Stat.HP, npc.Stats.Get(Stat.MaxHP));
            var config = new CombatGameState.Config
            {
                CanFlee = true,
                BackgroundPath = "",
                Party = GameState.World.Party.Members,
                Enemies = new System.Collections.Generic.List<RPG_Character.Actor> { npc },
                Stack = Stack,
                //OnWin
                //OnDie
            };
            ServiceManager.Get<Party>().PrepareForCombat();
            ServiceManager.Get<NPCManager>().PrepareForCombat();
            combat.Init(config);
            Stack.Push(combat);
            UIController.gameObject.SafeSetActive(true);
        }


        if (Input.GetKeyDown(KeyCode.K))
            ServiceManager.Get<GameStateManager>().SaveGameStateData(GameState.ToGameStateData());

        if (Input.GetKeyDown(KeyCode.L))
        {
            var gameManager = ServiceManager.Get<GameStateManager>();
            if (gameManager.GetNumberOfSaves() > 0)
                gameManager.LoadGameStateData(0);
        }

        if (Input.GetKeyDown(KeyCode.F2))
            GiveEverything();
        var deltaTime = Time.deltaTime;
        Stack.Update(deltaTime);
        GameState.World.Execute(deltaTime);
    }

    // TODO change name
    private void SetUpNewGame()
    {
        GameDataDownloader.LoadGameData(null);
    }

    private void LoadMap()
    {
        var obj = ServiceManager.Get<AssetManager>().Load<Map>(Constants.FIRST_VILLAGE_PREFAB_PATH);
        if (obj != null)
        {
            var map = Instantiate(obj);
            map.transform.SetParent(this.transform, false);
            //var exploreState = map.gameObject.AddComponent<ExploreState>();
            var exploreState = new ExploreState();
            exploreState.Init(map, Stack, Vector2.zero);
            Stack.Push(exploreState);
        }
    }

    private void GiveEverything()
    {
        var world = GameState.World;
        world.Gold = 999999;
        var gameData = ServiceManager.Get<GameData>();
        var items = gameData.Items;
        foreach(var entry in items.Dictionary)
        {
            var item = entry.Value;
            world.AddItem(item, 99);
        }

        var party = world.Party.Members;
        var spells = gameData.Spells;
        var specials = gameData.Specials;
        foreach (var member in party)
        {
            member.Spells.Clear();
            member.Specials.Clear();
            foreach (var spell in spells)
                member.Spells.Add(spell.Value);
            foreach (var special in specials)
                member.Specials.Add(special.Value);
        }
    }
}
