using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;
using RPG_GameState;
using RPG_UI;

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

    public static Action<Trigger, Entity> RemoveNPC(Map map, string npcId)
    {
        return (trigger, entity) =>
        {
            map.RemoveNPC(npcId);
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
     */

     public class StartCombatConfig
     {
        public bool CanFlee = true;
        public Map Map;
        public StateStack Stack;
        public Action OnWin;
        public Action OnLose;
        public List<string> Enemies = new List<string>();
        public List<Actor> Party = new List<Actor>();
    }

     public static void Combat(StartCombatConfig config, Vector3? OverrideCameraPosition = null)
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
        var combatConfig = new CombatGameState.Config
        {
            CanFlee = true,
            BackgroundPath = config.Map.CombatBackground,
            Party = config.Party,
            Enemies = CreateEnemyList(config.Map, config.Enemies),
            Stack = config.Stack,
            OnWin = config.OnWin,
            OnDie = config.OnLose
        };
        var events = new List<IStoryboardEvent>
        {
            StoryboardEventFunctions.BlackScreen(),
            StoryboardEventFunctions.FadeScreenIn("blackscreen", 0.5f),
            StoryboardEventFunctions.Function(() => 
            {
                ServiceManager.Get<Party>().PrepareForCombat();
                ServiceManager.Get<NPCManager>().PrepareForCombat();
                uiController.gameObject.SafeSetActive(true);
             }),
            StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.5f),
        };
        var storyboard = new Storyboard(config.Stack, events);
        var VirtualCam = Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        var CamAim = new GameObject("CameraAimPoint"); 
        
        if(OverrideCameraPosition != null)
           CamAim.transform.position = OverrideCameraPosition.Value;
       else
            CamAim.transform.position =  config.Party[0].transform.position + combatConfig.Enemies[0].transform.position/ 2; //this is gonna need some rewriting lol (i think at least).
        
        Debug.Log(combatConfig.Enemies[0].name);
        Debug.Log(combatConfig.Party[0].name);
        VirtualCam.m_Follow = CamAim.transform;
        
        //VirtualCam.PreviousStateIsValid = false;//.position = TargetCameraPosition;
        combat.Init(combatConfig);
        config.Stack.Push(combat);
        config.Stack.Push(storyboard);
        
    }

    private static List<Actor> CreateEnemyList(Map map, List<String> enemyList)
    {
        var enemies = new List<Actor>();
        if (enemyList == null || enemyList.Count < 1)
        {
            var enemy = LoadEnemy(map);
            if (enemy != null)
                enemies.Add(enemy);
            return enemies;
        }
        foreach (var enemy in enemyList)
        {
            var actor = LoadEnemy(map, enemy);
            if (actor != null)
                enemies.Add(actor);
        }
        return enemies;
    }

    private static Actor LoadEnemy(Map map, string enemyId = Constants.DEFAULT_COMBAT_ENEMY_PREFAB)
    {
        var assetManager = ServiceManager.Get<AssetManager>();
        var enemyData = ServiceManager.Get<GameData>().Enemies;
        if (!enemyData.ContainsKey(enemyId))
            return null;
        var enemyDef = enemyData[enemyId];
        string prefabPath = Constants.CHARACTER_PREFAB_PATH + enemyDef.PrefabPath;
        var asset = assetManager.Load<Actor>(prefabPath);
        var enemy = GameObject.Instantiate(asset);
        var character = enemy.GetComponent<Character>();
        character.Init(map, Constants.ENEMY_STATES);
        var actor = enemy.GetComponent<Actor>();
        actor.Init(enemyDef);
        return enemy;
    }
}