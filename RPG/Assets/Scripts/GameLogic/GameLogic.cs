using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] object GameState;// TODO GetDefaultGameState()
    [SerializeField] public InGameMenu GameMenu;
    [SerializeField] GameDataDownloader GameDataDownloader;

    private StateStack stack;
    private LocalizationManager localizationManager;

    private void Awake()
    {
        ServiceManager.Register(this);
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    private void Start()
    {
        stack = new StateStack();
        world.Reset();
        SetUpNewGame();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        stack.Update(deltaTime);
        world.Execute(deltaTime);
    }

    private void SetUpNewGame()
    {
        GameDataDownloader.DownLoadGameData(LoadMap);
    }

    private void LoadMap()
    {
        var obj = AssetManager.Load<Map>(Constants.TEST_MAP_PREFAB_PATH);
        if (obj != null)
        {
            var map = Instantiate(obj);
            map.transform.SetParent(this.transform, false);
            var exploreState = map.gameObject.AddComponent<ExploreState>();
            exploreState.Init(map, stack, Vector2.zero);
            stack.Push(exploreState);
        }
    }
}
