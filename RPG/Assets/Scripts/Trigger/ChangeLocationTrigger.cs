using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_Character;

public class ChangeLocationTrigger : MonoBehaviour
{
    [SerializeField] string NextMap;
    [SerializeField] Vector2 TeleportPosition;

    private void Awake()
    {
        if (NextMap.IsEmptyOrWhiteSpace())
        {
            gameObject.SafeSetActive(false);
            LogManager.LogWarn($"ChangeLocationTrigger's NextMap value is {NextMap}. Turning trigger off");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.tag.Equals("Player"))
            return;
        List<IStoryboardEvent> events;
        if (NextMap.Equals(Constants.WORLD_MAP_SCENE))
            events = GoToWorldMap();
        else
            events = GoToLocation();
        ServiceManager.Get<World>().LockInput();
        ServiceManager.Get<Party>().PrepareForSceneChange();
        var stack = ServiceManager.Get<GameLogic>().Stack;
        var storyboard = new Storyboard(stack, events, true);
        stack.Push(storyboard);
    }

    private List<IStoryboardEvent> GoToWorldMap()
    {
        var stack = ServiceManager.Get<GameLogic>().Stack;
        var currentScene = SceneManager.GetActiveScene().name;
        return Actions.GoToWorldMapEvents(stack, currentScene, Constants.WORLD_MAP_SCENE, 
            () => GameObject.Find($"{Constants.WORLD_MAP_SCENE}Map").GetComponent<Map>(), TeleportPosition);
    }

    private List<IStoryboardEvent> GoToLocation()
    {
        var stack = ServiceManager.Get<GameLogic>().Stack;
        var currentScene = SceneManager.GetActiveScene().name;
        return Actions.ChangeSceneEvents(stack, currentScene, NextMap, () => GameObject.Find($"{NextMap}Map").GetComponent<Map>());
    }
}
