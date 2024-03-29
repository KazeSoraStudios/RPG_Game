﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_UI;
using RPG_GameData;
using RPG_GameState;

public class GameLogic : MonoBehaviour
{
    [SerializeField] LogLevel LogLevel;
    [SerializeField] public GameState GameState = new GameState();
    [SerializeField] UIController UIController;
    [SerializeField] GameDataDownloader GameDataDownloader;
    [SerializeField] public GameSettings Settings;

    #if UNITY_EDITOR
    [SerializeField] string AreaId;
    [SerializeField] string EventId;
    #endif
  
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
            if (Settings.QuickPlay)
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
        if (Settings.QuickPlay)
        {
            StartCoroutine(StartQuickPlay());
        }
    }

    private IEnumerator StartQuickPlay()
    {
        yield return new WaitForSeconds(0.1f);
        var map = GameObject.FindObjectOfType<Map>();
            if (map == null)
            {
                LogManager.LogError("No map found for quick play.");
                yield break;
            }
            var exploreState = map.gameObject.AddComponent<ExploreState>();
            Stack.Push(exploreState);
            exploreState.Init(map, Stack, Vector2.zero);
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
        Stack.Push(exploreState);
        exploreState.Init(map, Stack, Vector2.zero);
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
        #if UNITY_EDITOR
        LogManager.SetLogLevel(LogLevel);

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameState.CompleteEventInArea(AreaId, EventId);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            var events = Actions.ChangeSceneEvents(Stack, "Village", "Forest", () => GameObject.Find("ForestMap").GetComponent<Map>());
            ServiceManager.Get<World>().LockInput();
            var storyboard = new Storyboard(Stack, events, true);
            Stack.Push(storyboard);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            var config = new Actions.StartCombatConfig
            { 
                CanFlee = true,
                Stack = Stack,
                Party = GameState.World.Party.Members,
                Enemies  = new List<string> { "firespirit" }
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
                #endif
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
    public bool ShowGameState;

    private Vector2 guiPosition = Vector2.zero;

    public void OnGUI()
    {
        guiPosition = Vector2.zero;
        if (ShowStackStates)
           RenderStack();
        if (ShowGameState)
            RenderGameState();
    }

    private void RenderStack()
    {
        float yDiff = 30.0f;
        GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), "StateStack:");
        guiPosition.y += yDiff;
        GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), $"Current Event: {Stack.Top().GetName()}");
        guiPosition.y += yDiff;

        var style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        if (Stack.IsEmpty())
            GUI.Label(new Rect(guiPosition, Vector2.one * 100.0f), "Empty!", style);
        var states = Stack.GetStates();
        for (int i = states.Count - 1; i >= 0; i--)
        {
            var message = $"{i} State: {states[i].GetName()}";
            GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), message, style);
            guiPosition.y += yDiff;
        }
    }

    private void RenderGameState()
    {
        var style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        float yDiff = 30.0f;
        GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), "GameState:", style);
        guiPosition.y += yDiff;
        GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), $"Gold: {GameState.World.Gold}, Time: {GameState.World.PlayTime}", style);
        guiPosition.y += yDiff;
        GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), $"Areas:", style);
        guiPosition.y += yDiff;
        var areas = GameState.Areas;
        foreach (var entry in areas)
        {
            var area = entry.Value;
            var message = area.CreateDebugAreaString();
            GUI.Label(new Rect(guiPosition, Vector2.one * 500.0f), message, style);
            guiPosition.y += yDiff;
        }
    }
#endif
}
