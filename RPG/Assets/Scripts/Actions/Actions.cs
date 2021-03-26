using System;
using RPG_Character;

public delegate void RunAction(params object[] args);

public class Actions
{
    //EquipSlotLabels =
    //{
    //    "Weapon:",
    //    "Armor:",
    //    "Accessory:",
    //    "Accessory:"
    //},
    //EquipSlotId =
    //{
    //    "weapon",
    //    "armor",
    //    "acces1",
    //    "acces2"
    //},
    //ActorStats =
    //{
    //    "strength",
    //    "speed",
    //    "intelligence"
    //},
    //ItemStats =
    //{
    //    "attack",
    //    "defense",
    //    "magic",
    //    "resist"
    //},
    //ActorStatLabels =
    //{
    //    "Strength",
    //    "Speed",
    //    "Intelligence"
    //},
    //ItemStatLabels =
    //{
    //    "Attack",
    //    "Defense",
    //    "Magic",
    //    "Resist"
    //},
    //ActionLabels =
    //{
    //    ["attack"] = "Attack",
    //    ["item"] = "Item",
    //    ["flee"] = "Flee",
    //    ["magic"] = "Magic",
    //    ["special"] = "Special"
    //}

    public static Action<Trigger, Entity> Teleport(Map map, int tileX, int tileY, int layer = 1)
    {
        return (trigger, entity) =>
        {
            entity.SetTilePosition(tileX, tileY, layer, map);
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
            ServiceManager.Get<NPCManager>().AddNPC(character);
        };
    }

    public static Action<Trigger, Entity> RemoveNPC(Map map, string npcId)
    {
        return (trigger, entity) =>
        {
            var character = ServiceManager.Get<NPCManager>().RemoveNPC(npcId);
            map.RemoveEntity(character.GetComponent<Entity>());
        };
    }

    /*

    AddSavePoint = function(map, x, y, layer)

        return function(trigger, entity, tX, tY, tLayer)


            local entityDef  =  gEntities["save_point"]
            local savePoint = Entity:Create(entityDef)
            savePoint:SetTilePos(x, y, layer, map)

            local function AskCallback(index)
                if index == 2 then
                    return
                end
                Save:Save()
                gGame.Stack:PushFit(gRenderer, 0, 0, "Saved!")
            end

            local trigger = Trigger:Create(
            {
                OnUse = function()
                    gGame.World.mParty:Rest()
                    local askMsg = "Save progress?"
                    gGame.Stack:PushFit(gRenderer, 0, 0, askMsg, false,
                    {
                        choices =
                        {
                            options = {"Yes", "No"},
                            OnSelection = AskCallback
                        },
                    })
                end
            })
            map:AddFullTrigger(trigger, x, y, layer)
        end

    end,

    AddChest = function(map, entityId, loot, x, y, layer)

        layer = layer or 1

        map.mContainerCount = map.mContainerCount or 0
        map.mContainerCount = map.mContainerCount + 1
        local containerId = map.mContainerCount
        local mapId = map.mMapDef.id
        local state = gGame.World.mGameState.maps[mapId]
        local isLooted = state.chests_looted[containerId] or false

        return function(trigger, entity, tX, tY, tLayer)

            local entityDef = gEntities[entityId]
            assert(entityDef ~= nil)
            local chest = Entity:Create(entityDef)

            chest:SetTilePos(x, y, layer, map)

            if isLooted then
                chest:SetFrame(entityDef.openFrame)
                return
            end

            local  OnOpenChest = function()

                if loot == nil or #loot == 0 then
                    gGame.Stack:PushFit(gRenderer, 0, 0, "The chest is empty!", 300)
                else

                    gGame.World:AddLoot(loot)

                    for _, item in ipairs(loot) do


                        local count = item.count or 1
                        local name = ItemDB[item.id].name
                        local message = string.format("Got %s", name)

                        if count > 1 then
                            message = message .. string.format(" x%d", count)
                        end

                        gGame.Stack:PushFit(gRenderer, 0, 0, message, 300)
                    end
                end

               map:RemoveTrigger(chest.mTileX, chest.mTileY, chest.mLayer)
               chest:SetFrame(entityDef.openFrame)
               print(string.format("Chest: %d set to looted.", containerId))
               state.chests_looted[containerId] = true
            end


            local trigger = Trigger:Create( { OnUse = OnOpenChest } )
            map:AddFullTrigger(trigger,
                               chest.mTileX, chest.mTileY, chest.mLayer)
        end
    end,

    RunScript = function(map, Func)
        return function(trigger, entity, tX, tY, tLayer)
            Func(map, trigger, entity, tX, tY, tLayer)
        end
    end,

    OpenShop = function(map, def)
        return function(trigger, entity, tX, tY, tLayer)
            gGame.Stack:Push(ShopState:Create(gGame.Stack, gGame.World, def))
        end
    end,

    OpenInn = function(map, def)

        def = def or {}
        local cost = def.cost or 5
        local lackGPMsg = "You need %d gp to stay at the Inn."
        local askMsg = "Stay at the inn for %d gp?"
        local resultMsg = "HP/MP Restored!"

        askMsg = string.format(askMsg, cost)
        lackGPMsg = string.format(lackGPMsg, cost)

        local OnSelection = function(index, item)
            print("selection! callback", index, item)
            if index == 2 then
                return
            end

            gGame.World.mGold = gGame.World.mGold - cost
            gGame.World.mParty:Rest()
            gGame.Stack:PushFit(gRenderer, 0, 0, resultMsg)
        end

        return function(trigger, entity, tX, tY, tLayer)

            local gp = gGame.World.mGold

            if gp >= cost then
                gGame.Stack:PushFit(gRenderer, 0, 0, askMsg, false,
                {
                    choices =
                    {
                        options = {"Yes", "No"},
                        OnSelection = OnSelection
                    },
                })
            else
                gGame.Stack:PushFit(gRenderer, 0, 0, lackGPMsg)
            end

        end
    end,

    ShortText = function(map, text)
        return function(trigger, entity, tX, tY, tLayer)
            tY = tY - 4
            local x, y = map:TileToScreen(tX, tY)
            gGame.Stack:PushFix(gRenderer, x, y, 9*32, 2.5*32, text)
        end
    end,

    ChangeMap = function(map, destinationId, dX, dY)

        local storyboard =
        {
            SOP.BlackScreen("blackscreen", 0),
            SOP.FadeInScreen("blackscreen", 0.5),
            SOP.ReplaceScene(
                "handin",
                {
                    map = destinationId,
                    focusX = dX,
                    focusY = dY,
                    hideHero = false
                }),
            SOP.FadeOutScreen("blackscreen", 0.5),
            SOP.Function(function()
                            gGame.World:UnlockInput()
                         end),
            SOP.HandOff(destinationId),
        }

        return function(trigger, entity, tX, tY, tLayer)
            gGame.World:LockInput()
            local storyboard = Storyboard:Create(gGame.Stack, storyboard, true)
            gGame.Stack:Push(storyboard)
        end
    end,

    Combat = function(map, def)
        return function(trigger, entity, tX, tY, tLayer)

            def.background = def.background or "combat_bg_field.png"
            def.enemy = def.enemy or { "grunt" }

            local enemyList = {}
            for k, v in ipairs(def.enemy) do
                print(v, tostring(gEnemyDefs[v]))
                local enemyDef = gEnemyDefs[v]
                enemyList[k] = Actor:Create(enemyDef)
            end

            local combatState = CombatState:Create(gGame.Stack,
            {
                background = def.background,
                actors =
                {
                    party = gGame.World.mParty:ToArray(),
                    enemy = enemyList,
                },
                canFlee = def.canFlee,
                OnWin = def.OnWin,
                OnDie = def.OnDie

            })

            local storyboard =
            {
                SOP.BlackScreen("blackscreen", 0),
                SOP.FadeInScreen("blackscreen", 0.5),
                SOP.Function(
                    function()
                        gGame.Stack:Push(combatState)
                    end)
            }

            local storyboard = Storyboard:Create(gGame.Stack, storyboard)
            gGame.Stack:Push(storyboard)
        end
    end


     */
}