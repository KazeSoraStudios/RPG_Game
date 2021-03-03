using System.Collections;
using UnityEngine;

public class ExploreState : MonoBehaviour, IGameState
{
    public bool followCamera = true;
    public int manualCameraX = 0;
    public int manualCameraY = 0;
    public Character followCharacter;
    public Character Hero;
    public StateStack stack;
    public Map Map;

    public void Init(Map map, StateStack stack, Vector2 startPosition)
    {
        this.Map = map;
        this.stack = stack;
        var obj = ServiceManager.Get<AssetManager>().Load<Character>(Constants.HERO_PREFAB_PATH);
        if (obj != null)
        {
            Hero = Instantiate(obj);
            Hero.transform.position = Vector2.zero;
            Hero.transform.rotation = Quaternion.identity;
            followCharacter = Hero;
            Hero.gameObject.transform.SetParent(transform, true);
            Hero.Init(map);
            var actor = Hero.GetComponent<Actor>();
            actor.Init(ServiceManager.Get<GameData>().PartyDefs["hero"]);
            ServiceManager.Get<World>().Party.Add(actor);
        }
        map.GoToTile((int)startPosition.x, (int)startPosition.y);
    }

    public void HideHero()
    {
        Hero.Entity.Hide();
    }

    public void ShowHero()
    {
        Hero.Entity.Show();
    }

    public void UpdateCamera(Map map)
    {
        if (followCamera)
        {
            var position = Hero.Entity.transform.position;
            map.UpdateCameraPosition((int)position.x, (int)position.y);
        }
        else
        {
            map.UpdateCameraPosition(manualCameraX, manualCameraY);
        }
    }

    public void SetFollowCamera(bool followPlayer, Character character = null)
    {
        if (character != null)
            followCharacter = character;
        followCamera = followPlayer;
        if (followCamera)
            return;
        var position = followCharacter.Entity.transform.position;
        Map.UpdateCameraPosition((int)position.x, (int)position.y);
    }

    public void Enter(object o) { }

    public void Exit() { }

    public bool Execute(float deltaTime)
    {
        UpdateCamera(Map);
        foreach (var npc in Map.NpcsById.Values)
            npc.Controller.Update(deltaTime);
        return true;
    }

    public void HandleInput()
    {
        if (ServiceManager.Get<World>().IsInputLocked())
        {
            LogManager.LogDebug("Input locked, not handling ExploreState input.");
            return;
        }
        Hero.Controller.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var facingTile = Hero.GetFacingTilePosition();
            var x = (int)facingTile.x;
            var y = (int)facingTile.y;
            if (Map.GetTrigger(x, y) is var trigger && trigger != null)
                trigger.OnUse(new TriggerParams(x,y,Hero));
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            // TODO ingame menu
            //local menu = InGameMenuState:Create(self.mStack, self.mMapDef)
            //return self.mStack:Push(menu)
            var menu = ServiceManager.Get<GameLogic>().GameMenu;
            menu.Init(Map, stack);
            stack.Push(menu);
        }
    }

    public string GetName()
    {
        return "ExploreState";
    }
}