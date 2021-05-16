using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RPG_Character;

public class Map : MonoBehaviour
{
    [SerializeField] public string MapName;
    [SerializeField] Dictionary<Vector2Int, Trigger> Triggers = new Dictionary<Vector2Int, Trigger>();
    [SerializeField] Dictionary<Vector2Int, Entity> Entities = new Dictionary<Vector2Int, Entity>();
    [SerializeField] Tilemap collision;

    private EmptyTrigger emptyTrigger = new EmptyTrigger();

    public struct ChangeTile
    {
        public bool collision;
        public int x, y, layer;
        public Tile sprite, detail;
    }

    public bool IsCollision(Vector2 position)
    {
        var tilePosition = new Vector3Int((int)position.x, (int)position.y, 0);
        return collision.HasTile(tilePosition);
    }

    public Trigger GetTrigger(int x, int y)
    {
        var position = new Vector2Int(x, y);
        var trigger = emptyTrigger;
        if (!Triggers.ContainsKey(position))
            return trigger;
        return Triggers[position];
    }

    public void AddTrigger(int x, int y, Trigger trigger)
    {
        var position = new Vector2Int(x, y);
        Triggers.Add(position, trigger);
    }

    public void AddTrigger(Vector2Int position, Trigger trigger)
    {
        Triggers.Add(position, trigger);
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

    /*
     

    this.mCamX = 0
    this.mCamY = 0

    this.mWidthPixel = this.mWidth * this.mTileWidth
    this.mHeightPixel = this.mHeight * this.mTileHeight

    this.mActions = {}
    for name, def in pairs(mapDef.actions or {}) do
        assert(Actions[def.id])
        local action = Actions[def.id](this, unpack(def.params))
        this.mActions[name] = action
    end

    this.mTriggerTypes = {}
    for k, v in pairs(mapDef.trigger_types or {}) do
        local triggerParams = {}
        for callback, action in pairs(v) do
            triggerParams[callback] = this.mActions[action]
            assert(triggerParams[callback])
        end
        this.mTriggerTypes[k] = Trigger:Create(triggerParams)
    end

    local encounterData = mapDef.encounters or {}
    this.mEncounters = {}
    for k, v in ipairs(encounterData) do
        local oddTable = OddmentTable:Create(v)
        this.mEncounters[k] = oddTable
    end
    print("Oddment size", #this.mEncounters)


    setmetatable(this, self)

    this.mTriggers = {}
    for k, v in ipairs(mapDef.triggers) do
        this:AddTrigger(v)
    end

    for _, v in ipairs(mapDef.on_wake or {}) do
        local action = Actions[v.id]
        action(this, unpack(v.params))()
    end

    return this
end




function Map:AddFullTrigger(trigger, x, y, layer)

    layer = layer or 1
    if not self.mTriggers[layer] then
        self.mTriggers[mLayer] = {}
    end

    local targetLayer = self.mTriggers[layer]
    targetLayer[self:CoordToIndex(x, y)] = trigger

end

function Map:GetEntity(x, y, layer)
    if not self.mEntities[layer] then
        return nil
    end
    local index = self:CoordToIndex(x, y)
    return self.mEntities[layer][index]
end

function Map:AddEntity(entity)

    if not self.mEntities[entity.mLayer] then
        self.mEntities[entity.mLayer] = {}
    end

    local layer = self.mEntities[entity.mLayer]
    local index = self:CoordToIndex(entity.mTileX, entity.mTileY)

    assert(layer[index] == entity or layer[index] == nil)
    layer[index] = entity
end

function Map:RemoveEntity(entity)
    assert(self.mEntities[entity.mLayer])
    local layer = self.mEntities[entity.mLayer]
    local index = self:CoordToIndex(entity.mTileX, entity.mTileY)
    assert(entity == layer[index])
    layer[index] = nil
end

function Map:GetTile(x, y, layer)
    local layer = layer or 1
    local tiles = self.mMapDef.layers[layer].data

    return tiles[self:CoordToIndex(x, y)]
end

function Map:GetTrigger(x, y, layer)
    layer = layer or 1
    local triggers = self.mTriggers[layer]

    if not triggers then
        return
    end

    local index = self:CoordToIndex(x, y)
    return triggers[index]
end

function Map:RemoveTrigger(x, y, layer)
    layer = layer or 1
    assert(self.mTriggers[layer])
    local triggers = self.mTriggers[layer]
    local index = self:CoordToIndex(x, y)
    assert(triggers[index])
    triggers[index] = nil
end

function Map:GetNPC(x, y, layer)
    layer = layer or 1

    for _, npc in ipairs(self.mNPCs) do
        if npc.mEntity.mTileX == x
            and npc.mEntity.mTileY == y
            and npc.mEntity.mLayer == layer then
            return npc
        end
    end

    return nil

end

function Map:RemoveNPC(x, y, layer)
    layer = layer or 1

    for i = #self.mNPCs, 1, -1 do
        local npc = self.mNPCs[i]
        if npc.mEntity.mTileX == x
            and npc.mEntity.mTileY == y
            and npc.mEntity.mLayer == layer then
            table.remove(self.mNPCs, i)
            self:RemoveEntity(npc.mEntity)
            self.mNPCbyId[npc.mId] = nil
            return true
        end
    end

    return false
end

function Map:CoordToIndex(x, y)
    x = x + 1             
    return x + y * self.mWidth
end

function Map:IsBlocked(layer, tileX, tileY)
    -- Collision layer is 2 above the official layer
    local tile = self:GetTile(tileX, tileY, layer + 2)
    local entity = self:GetEntity(tileX, tileY, layer)
    return tile == self.mBlockingTile or entity ~= nil
end

function Map:GotoTile(x, y)
    print("Goto tile:", x, y)
    self:Goto((x * self.mTileWidth) + self.mTileWidth/2,
              (y * self.mTileHeight) + self.mTileHeight/2)
end

function Map:Goto(x, y)
    self.mCamX = x - System.ScreenWidth()/2
    self.mCamY = -y + System.ScreenHeight()/2
end

function Map:PointToTile(x, y)
    x = x + self.mTileWidth * 0.5
    y = y - self.mTileHeight * 0.5

    x = math.max(self.mX, x)
    y = math.min(self.mY, y)
    x = math.min(self.mX + self.mWidthPixel - 1, x)
    y = math.max(self.mY- self.mHeightPixel + 1, y)


    local tileX = math.floor((x - self.mX) / self.mTileWidth)
    local tileY = math.floor((self.mY - y) / self.mTileHeight)

    return tileX, tileY
end

function Map:LayerCount()
    assert(#self.mMapDef.layers % 3 == 0)
    return #self.mMapDef.layers / 3
end


function Map:Render(renderer)
    self:RenderLayer(renderer, 1)
end

function Map:RenderLayer(renderer, layer, hero)


    local layerIndex = (layer * 3) - 2

    local tileLeft, tileBottom =
        self:PointToTile(self.mCamX - System.ScreenWidth() * 0.5,
                         self.mCamY - System.ScreenHeight() * 0.5)

    local tileRight, tileTop =
        self:PointToTile(self.mCamX + System.ScreenWidth() * 0.5,
                         self.mCamY + System.ScreenHeight() * 0.5)

    for j = tileTop, tileBottom do
        for i = tileLeft, tileRight do

            local tile = self:GetTile(i, j, layerIndex)
            local uvs = {}


            self.mTileSprite:SetPosition(self.mX + i * self.mTileWidth,
                                         self.mY - j * self.mTileHeight)

            if tile > 0 then
				uvs = self.mUVs[tile]

                assert(uvs, string.format("No uvs for tile [%d]", tile))

                self.mTileSprite:SetUVs(unpack(uvs))
                renderer:DrawSprite(self.mTileSprite)
            end

            tile = self:GetTile(i, j, layerIndex + 1)

            if tile > 0 then
                uvs = self.mUVs[tile]
                self.mTileSprite:SetUVs(unpack(uvs))
                renderer:DrawSprite(self.mTileSprite)
            end
        end

        local entLayer = self.mEntities[layer] or {}
        local drawList = { hero }

        for _, j in pairs(entLayer) do
            table.insert(drawList, j)
        end

        table.sort(drawList, function(a, b) return a.mTileY < b.mTileY end)
        for _, j in ipairs(drawList) do
            j:Render(renderer)
        end
    end
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
