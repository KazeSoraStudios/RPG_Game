using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_UI;
using RPG_Combat;
using RPG_Character;
using RPG_GameData;
using RPG_GameState;
using RPG_Audio;

public class GameLogic : MonoBehaviour
{
    [SerializeField] bool QuickPlay;
    [SerializeField] LogLevel LogLevel;
    [SerializeField] public GameState GameState;
    [SerializeField] UIController UIController;
    [SerializeField] GameDataDownloader GameDataDownloader;
  
    public StateStack Stack;

    private void Awake()
    {
        ServiceManager.Register(this);
        LogManager.SetLogLevel(LogLevel);
        DontDestroyOnLoad(this);
        //DontDestroyOnLoad(Camera.main);
        var ui = GameObject.Find("UICanvas");
        if (ui == null)
        {
            if (QuickPlay)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/UI/UI");
                ui = Instantiate(prefab);
            }
            if (ui == null)
            {
                LogManager.LogError("No UI in scene.");
                return;
            }
        }
        if (UIController == null)
        {
            UIController = ui.GetComponent<UIController>();
            DontDestroyOnLoad(UIController);
        }
        else
        {
            DontDestroyOnLoad(UIController.transform.parent);
        }

        GameState = new GameState
        {
            World = GetComponent<World>()
        };
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    private void Start()
    {
        Stack = new StateStack();
        UIController.InitUI();
        LoadData();
        if (QuickPlay)
        {
            var map = GameObject.FindObjectOfType<Map>();
            if (map == null)
            {
                LogManager.LogError("No map found for quick play.");
                return;
            }
            var exploreState = map.gameObject.AddComponent<ExploreState>();
            exploreState.Init(map, Stack, Vector2.zero);
            Stack.Push(exploreState);
        }
    }

    public void StartNewGame()
    {
        StartCoroutine(LoadScene(Constants.HERO_VILLAGE_SCENE));
    }

    IEnumerator LoadScene(string scene, Action<ExploreState> callback = null)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        yield return new WaitForSeconds(0.1f);
        var village = GameObject.Find($"{scene}Map");
        var map = village.GetComponent<Map>();
        var exploreState = map.gameObject.AddComponent<ExploreState>();
        exploreState.Init(map, Stack, Vector2.zero);
        Stack.Push(exploreState);
        callback?.Invoke(exploreState);
        yield return null;
    }

    public void LoadGame()
    {
        var gameManager = ServiceManager.Get<GameStateManager>();
        GameState = gameManager.LoadGameStateFromCurrentData(GetComponent<World>());
        var savedData = gameManager.GetCurrent();
        var events = Actions.LoadGameEvents(Stack, savedData);
        var storyboard = new Storyboard(Stack, events, false);
        Stack.Push(storyboard);
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
            var events = Actions.ChangeSceneEvents(Stack, "Village", "Forest", () => GameObject.Find("ForestMap").GetComponent<Map>());
            ServiceManager.Get<World>().LockInput();
            var storyboard = new Storyboard(Stack, events, true);
            Stack.Push(storyboard);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            //var parent = UIController.CombatLayer;
            //var obj = ServiceManager.Get<AssetManager>().Load<CombatGameState>(Constants.COMBAT_PREFAB_PATH);
            //var combat = Instantiate(obj, Vector3.zero, Quaternion.identity);
            //combat.transform.SetParent(parent, false);
            //var npc = GameObject.Find("TestNPC(Clone)").AddComponent<Actor>();
            //npc.Init(ServiceManager.Get<GameData>().Enemies["goblin"]);
            //npc.Stats.SetStat(Stat.HP, npc.Stats.Get(Stat.MaxHP));
            //var config = new CombatGameState.Config
            //{
            //    CanFlee = true,
            //    BackgroundPath = "",
            //    Party = GameState.World.Party.Members,
            //    Enemies = new System.Collections.Generic.List<RPG_Character.Actor> { npc },
            //    Stack = Stack,
            //    //OnWin
            //    //OnDie
            //};
            //ServiceManager.Get<Party>().PrepareForCombat();
            //ServiceManager.Get<NPCManager>().PrepareForCombat();
            //combat.Init(config);
            //Stack.Push(combat);
            //UIController.gameObject.SafeSetActive(true);
            var config = new Actions.StartCombatConfig
            { 
                CanFlee = true,
                Map = ((ExploreState)Stack.Top()).Map,
                Stack = Stack,
                Party = GameState.World.Party.Members,
                Enemies  = new List<string> { "goblin" }
            };
            Actions.Combat(config);
        }


        if (Input.GetKeyDown(KeyCode.K))
            ServiceManager.Get<GameStateManager>().SaveGameStateData(GameState.Save());

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

    private void LoadData()
    {
        GameDataDownloader.LoadGameData(null);
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

    #if UNITY_EDITOR
    public bool ShowStackStates = false;

    public void OnGUI()
    {
        if (!ShowStackStates)
            return;
        float yDiff = 30.0f;
        var position = Vector2.zero;
        GUI.Label(new Rect(position, Vector2.one * 500.0f), "StateStack:");
        position.y += yDiff;
        GUI.Label(new Rect(position, Vector2.one * 500.0f), $"Current Event: {Stack.Top().GetName()}");
        position.y += yDiff;

        var style = new GUIStyle();
        style.fontSize = 24;
        if (Stack.IsEmpty())
            GUI.Label(new Rect(position, Vector2.one * 100.0f), "Empty!", style);
        var states = Stack.GetStates();
        for (int i = states.Count - 1; i >= 0; i--)
        {
            var message = $"{i} State: {states[i].GetName()}";
            GUI.Label(new Rect(position, Vector2.one * 500.0f), message, style);
            position.y += yDiff;
        }
    }
#endif
}
