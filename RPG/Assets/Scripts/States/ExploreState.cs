using UnityEngine;
using RPG_UI;
using RPG_Character;
using RPG_GameData;

public class ExploreState : MonoBehaviour, IGameState
{
    public bool followCamera = true;
    public Character followCharacter;
    public Character Hero;
    public StateStack stack;
    public Map Map;

    private void OnDestroy() 
    {
        var npcManager = ServiceManager.Get<NPCManager>();
        if (npcManager == null)
            return;
        npcManager.ClearNpcsForMap(Map.MapName);
    }

    public void Init(Map map, StateStack stack, Vector3 startPosition)
    {
        this.Map = map;
        this.stack = stack;
        LoadHero(startPosition);
        Map.Init();
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
        
    }

    public void Enter(object o) { }

    public void Exit() 
    {
        
    }

    public bool Execute(float deltaTime)
    {
        UpdateCamera(Map);
        ServiceManager.Get<NPCManager>().UpdateAllNPCsforMap(Map.MapName, deltaTime);
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
            var targetPosition = new Vector2(x,y);
            var collision = Physics2D.OverlapCircle(targetPosition, 0.2f, Hero.collisionLayer);
            if (collision?.GetComponent<Trigger>() is var trigger && trigger != null)
            {
                trigger.OnUse(new TriggerParams(x,y,Hero.GetComponent<Character>()));
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ServiceManager.Get<UIController>().OpenMenu(Map, stack);
        }
    }

    public string GetName()
    {
        return "ExploreState: " + name;
    }

    public void SetHeroPosition(Vector2 position)
    {
        if (Hero == null)
            return;
        Hero.transform.position = position;
    }

    private void LoadHero(Vector3 startPosition)
    {
        var game = ServiceManager.Get<GameLogic>().GameState;
        if (game == null || game.World.Party.Count() < 1)
            LoadHeroPrefab();
        else
            Hero = game.World.Party.GetActor(0).GetComponent<Character>();
        Hero.Map = Map;
        Hero.transform.position = startPosition;
        Hero.transform.rotation = Quaternion.identity;
        Map.Camera.Follow = Hero.transform;
    }

    private void LoadHeroPrefab()
    {
        var obj = ServiceManager.Get<AssetManager>().Load<Character>(Constants.HERO_PREFAB);
        if (obj == null)
        {
            LogManager.LogError("Unable to load hero in ExplroeState.");
            return;
        }
        Hero = GameObject.Instantiate(obj);
        followCharacter = Hero;
        var world = ServiceManager.Get<World>();
        var characterParent = world.PersistentCharacters;
        Hero.gameObject.transform.SetParent(characterParent, true);
        Hero.Init(Map, Constants.PARTY_STATES, Constants.WAIT_STATE);
        var actor = Hero.GetComponent<Actor>();
        actor.Init(ServiceManager.Get<GameData>().PartyDefs["hero"]);
        world.Party.Add(actor);
    }
}