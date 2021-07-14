using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RPG_Character;
using RPG_GameData;
using Cinemachine;

public class Map : MonoBehaviour
{
    [SerializeField] public string MapName;
    [SerializeField] public string CombatBackground = Constants.DEFAULT_COMBAT_BACKGROUND;
    [SerializeField] public Vector3 HeroStartingPosition;
    [SerializeField] public CinemachineVirtualCamera Camera;
    [SerializeField] Transform NPCParent;
    [SerializeField] Tilemap Encounters;
    [SerializeField] Dictionary<Vector2Int, Entity> Entities = new Dictionary<Vector2Int, Entity>();
    [SerializeField] List<NPCData> MapNPCs = new List<NPCData>();

    private Encounter Encounter = new Encounter();

    private void Start()
    {
        var encounters = ServiceManager.Get<GameData>().Encounters;
        var map = MapName.ToLower();
        if (encounters.ContainsKey(map))
        {
            Encounter = encounters[map];
        }
    }

    public void LoadNpcs()
    {
        var assetManager = ServiceManager.Get<AssetManager>();
        foreach (var data in MapNPCs)
        {
            var asset = assetManager.Load<Character>(Constants.CHARACTER_PREFAB_PATH + data.PrefabId);
            if (asset == null)
            {
                continue;
            }
            var npc = GameObject.Instantiate(asset);
            npc.transform.position = data.StartingPosition;
            npc.transform.rotation = Quaternion.identity;
            AddNPC(npc);
            var defaultState = data.DefaultState.IsEmptyOrWhiteSpace() ? Constants.WAIT_STATE : data.DefaultState.ToUpper();
            npc.Init(this, Constants.ENEMY_STATES, defaultState);
        }
    }

    public void AddNPC(Character npc)
    {
        npc.transform.SetParent(NPCParent, false);
        ServiceManager.Get<NPCManager>().AddNPC(MapName, npc);
    }

    public void AddCombatNPC(Character npc)
    {
        npc.transform.SetParent(NPCParent, false);
        ServiceManager.Get<NPCManager>().AddNPC("combat", npc);
    }

    public void RemoveNPC(Character npc)
    {
        var character = ServiceManager.Get<NPCManager>().RemoveNPC(MapName, npc);
        Destroy(character.gameObject);
    }

    public void AddEntity(Entity entity)
    {
        var pos = entity.transform.position;
        var position = new Vector2Int((int)pos.x, (int)pos.y);
        if (Entities.ContainsKey(position))
            return;
        Entities.Add(position, entity);
    }

    public void AddEntity(Entity entity, Vector2Int position)
    {
        if (Entities.ContainsKey(position))
            return;
        Entities.Add(position, entity);
    }

    public void RemoveEntity(Entity entity)
    {
        var pos = entity.transform.position;
        var position = new Vector2Int((int)pos.x, (int)pos.y);
        if (!Entities.ContainsKey(position))
            return;
        Entities.Remove(position);
    }

    public Entity GetEntity(Vector2 pos)
    {
        var position = new Vector2Int((int)pos.x, (int)pos.y);
        if (!Entities.ContainsKey(position))
            return null;
        return Entities[position];
    }

    public bool TryEncounter(Vector3Int position)
    {
        if (Encounters == null)
            return false;
        if (!Encounters.HasTile(position))
            return false;
        var tile = Encounters.GetTile(position) as EncounterTile;
        if (tile == null)
            return false;
        var encounter = Encounter.GetEncounter(tile.EncounterId).Pick();
        if (encounter.Items.Count == 0)
            return false;
        StartCombat(encounter.Items);
        return true;
    }

    private void StartCombat(List<string> enemies)
    {
        var gameLogic = ServiceManager.Get<GameLogic>();
        var config = new Actions.StartCombatConfig
        { 
            CanFlee = true,
            Map = this,
            Stack = gameLogic.Stack,
            Party = gameLogic.GameState.World.Party.Members,
            Enemies  = enemies
        };
        Actions.Combat(config);
    }

    /*
    local encounterData = mapDef.encounters or {}
    this.mEncounters = {}
    for k, v in ipairs(encounterData) do
        local oddTable = OddmentTable:Create(v)
        this.mEncounters[k] = oddTable
    end
function Map:TryEncounter(x, y, layer)

    local tile = self:GetTile(x, y, layer + 2)
    if tile <= self.mBlockingTile then
        return
    end

    local index = tile - self.mBlockingTile
    local odd = self.mEncounters[index]

    if not odd then
        return
    end

    local encounter = odd:Pick()

    if not next(encounter) then
        return
    end

    print(string.format("%d tile id. index: %d", tile, index))
    local action = Actions.Combat(self, encounter)
    action(nil, nil, x, y, layer)

end


     */
}
