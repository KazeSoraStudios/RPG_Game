using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_AI;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;
using RPG_GameState;
using RPG_UI;

public delegate void RunAction(params object[] args);

public class Actions
{    
    public static void Teleport(Entity hero, Vector2 position)
    {
        ServiceManager.Get<World>().LockInput();
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => hero.SetTilePosition(position)),
            StoryboardEventFunctions.Wait(0.5f),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().UnlockInput()),
            StoryboardEventFunctions.HandOffToExploreState()
        };
        var stack = ServiceManager.Get<GameLogic>().Stack;
        var storyboard = new Storyboard(stack, events, true);
        stack.Push(storyboard);
    }

    public static List<IStoryboardEvent> ChangeSceneEvents(StateStack stack, string currentScene, string nextScene, Func<Map> function)
    {
        return new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.LoadScene(nextScene),
            //StoryboardEventFunctions.Wait(1.0f),
            StoryboardEventFunctions.ReplaceExploreState(Constants.HANDIN_STATE, stack, function),
            StoryboardEventFunctions.DeleteScene(currentScene, nextScene),
            //StoryboardEventFunctions.Wait(2.0f),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().UnlockInput()),
            StoryboardEventFunctions.HandOffToExploreState()
        };
    }

    public static List<IStoryboardEvent> LoadGameEvents(StateStack stack, GameStateData data)
    {
        return new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.LoadScene(data.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single),
            //StoryboardEventFunctions.Wait(1.0f),
            StoryboardEventFunctions.AddExploreStateToCurrentMap(data.sceneName, stack),
            StoryboardEventFunctions.MoveHeroToPosition(data.sceneName, data.location),
            //StoryboardEventFunctions.Wait(2.0f),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().UnlockInput()),
           StoryboardEventFunctions.HandOffToExploreState(data.sceneName)
        };
    }

    public static List<IStoryboardEvent> GoToWorldMapEvents(StateStack stack, string currentScene, string nextScene, Func<Map> function, Vector2 position)
    {
        return new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.LoadScene(nextScene),
            //StoryboardEventFunctions.Wait(1.0f),
            StoryboardEventFunctions.ReplaceExploreState(Constants.HANDIN_STATE, stack, function),
            StoryboardEventFunctions.MoveHeroToPosition(nextScene, position),
            StoryboardEventFunctions.DeleteScene(currentScene, nextScene),
            //StoryboardEventFunctions.Wait(2.0f),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => ServiceManager.Get<World>().UnlockInput()),
            StoryboardEventFunctions.HandOffToExploreState()
        };
    }

    public static Action<Trigger, Entity, int, int, int> AddNPC(Map map, Entity npc)
    {
        LogManager.LogDebug($"Adding NPC [{npc.name}]");
        return (trigger, entity, tX, tY, tLayer) =>
        {
            // TODO get character info and populate information
            var character = entity.gameObject.GetComponent<Character>();
            entity.SetTilePosition(tX, tY, tLayer, map);
            map.AddNPC(character);
        };
    }

    public static Action<Trigger, Entity> RemoveNPC(Map map, Character npc)
    {
        return (trigger, entity) =>
        {
            map.RemoveNPC(npc);
        };
    }

    public static void SetCameraToCombatPosition()
    {
        var camera = Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        camera.m_Follow = ServiceManager.Get<CombatScene>().CameraPosition;
        camera.ForceCameraPosition(camera.m_Follow.position, Quaternion.identity);
    }

    public static void SetCameraToFollowHero()
    {
        var camera = Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        var hero = ServiceManager.Get<Party>().Members[0].transform;;
        camera.m_Follow = hero;
        camera.ForceCameraPosition(hero.position, Quaternion.identity);
    }

     public class StartCombatConfig
     {
        public bool CanFlee = true;
        public string Background;
        public StateStack Stack;
        public Action OnWin;
        public Action OnLose;
        public List<string> Enemies = new List<string>();
        public List<Actor> Party = new List<Actor>();
    }

     public static void Combat(StartCombatConfig config)
     {
        var uiController = ServiceManager.Get<UIController>();
        var combatUIParent = uiController.CombatLayer;
        var asset = ServiceManager.Get<AssetManager>().Load<CombatGameState>(Constants.COMBAT_PREFAB_PATH);
        if (asset == null)
        {
            LogManager.LogError($"Unable to load Combat: {Constants.COMBAT_PREFAB_PATH}");
            return;
        }
        var combat = GameObject.Instantiate(asset, Vector3.zero, Quaternion.identity);
        combat.transform.SetParent(combatUIParent, false);
        combat.gameObject.SafeSetActive(false);
        var combatConfig = new CombatGameState.Config
        {
            CanFlee = true,
            BackgroundPath = config.Background,
            Party = config.Party,
            Enemies = CreateEnemyList(config.Enemies),
            Stack = config.Stack,
            OnWin = config.OnWin,
            OnDie = config.OnLose
        };
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.Wait(0.5f),
            StoryboardEventFunctions.Function(() => 
            {
                SetCameraToCombatPosition();
                ServiceManager.Get<Party>().PrepareForCombat();
                ServiceManager.Get<NPCManager>().PrepareForCombat();
                combat.Init(combatConfig);
             }),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => combat.gameObject.SafeSetActive(true))
        };
        var storyboard = new Storyboard(config.Stack, events);
        config.Stack.Push(combat);
        config.Stack.Push(storyboard);
    }

    private static List<Actor> CreateEnemyList(List<String> enemyList)
    {
        var enemies = new List<Actor>();
        if (enemyList == null || enemyList.Count < 1)
        {
            var enemy = LoadEnemy();
            if (enemy != null)
                enemies.Add(enemy);
            return enemies;
        }
        foreach (var enemy in enemyList)
        {
            var actor = LoadEnemy(enemy);
            if (actor != null)
                enemies.Add(actor);
        }
        return enemies;
    }

    private static Actor LoadEnemy(string enemyId = Constants.DEFAULT_COMBAT_ENEMY_PREFAB)
    {
        var assetManager = ServiceManager.Get<AssetManager>();
        var enemyData = ServiceManager.Get<GameData>().Enemies;
        if (!enemyData.ContainsKey(enemyId))
        {
            LogManager.LogError($"EnemyId {enemyId} is not found in GameData Enemies");
            return null;
        }
        var enemyDef = enemyData[enemyId];
        string prefabPath = Constants.CHARACTER_PREFAB_PATH + enemyDef.PrefabPath;
        var asset = assetManager.Load<Actor>(prefabPath);
        var enemy = GameObject.Instantiate(asset);
        var character = enemy.GetComponent<Character>();
        character.Init(Constants.ENEMY_STATES);
        var actor = enemy.GetComponent<Actor>();
        actor.Init(enemyDef);
        return enemy;
    }
}