using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_UI;
public class GameLogic : MonoBehaviour
{
    [SerializeField] LogLevel LogLevel;
    [SerializeField] World world;
    [SerializeField] object GameState;// TODO GetDefaultGameState()
    [SerializeField] public InGameMenu GameMenu;
    [SerializeField] GameDataDownloader GameDataDownloader;
    [SerializeField] public Image ScreenImage;
    [SerializeField] public Textbox Textbox;

    public StateStack Stack;

    private LocalizationManager localizationManager;

    private void Awake()
    {
        ServiceManager.Register(this);
        LogManager.SetLogLevel(LogLevel);
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    private void Start()
    {
        Stack = new StateStack();
        Textbox.SetUp(Stack);
        world.Reset();
        SetUpNewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            var events = new List<IStoryboardEvent>
            {
                StoryboardEventFunctions.BlackScreen(),
                StoryboardEventFunctions.FadeScreenOut(),
                StoryboardEventFunctions.Wait(2.0f),
                //StoryboardEventFunctions.FadeScreenIn(),
                StoryboardEventFunctions.HandOffToExploreState()
            };
            var storyboard = new Storyboard(Stack, events, true);
            Stack.Push(storyboard);
            LogManager.LogInfo("Pushed storyboard");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Stack.PushTextbox("this is a sample text box", Constants.PORTRAIT_PATH + "mage_portrait");
        }

        if (Input.GetKeyDown(KeyCode.F2))
            GiveAllItems();
        var deltaTime = Time.deltaTime;
        Stack.Update(deltaTime);
        world.Execute(deltaTime);
    }

    private void SetUpNewGame()
    {
        GameDataDownloader.LoadGameData(LoadMap);
    }

    private void LoadMap()
    {
        var obj = ServiceManager.Get<AssetManager>().Load<Map>(Constants.TEST_MAP_PREFAB_PATH);
        if (obj != null)
        {
            var map = Instantiate(obj);
            map.transform.SetParent(this.transform, false);
            var exploreState = map.gameObject.AddComponent<ExploreState>();
            exploreState.Init(map, Stack, Vector2.zero);
            Stack.Push(exploreState);
        }
    }

    private void GiveAllItems()
    {
        var items = ServiceManager.Get<GameData>().Items;
        foreach(var item in items)
        {
            world.AddItem(item.Value, 99);
        }
    }
}
