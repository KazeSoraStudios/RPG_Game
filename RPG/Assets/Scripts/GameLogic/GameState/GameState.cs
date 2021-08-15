using System.Collections.Generic;
using UnityEngine.SceneManagement;
using RPG_GameData;

namespace RPG_GameState
{
    public class GameState
    {
        public World World;

        public Dictionary<string, Area> Areas = new Dictionary<string, Area>();

        public void Start()
        {
            World.Reset();
        }

        public GameStateData Save()
        {
            var config = new GameStateData.Config
            {
                Gold = World.Gold,
                PlayTime = World.PlayTime,
                Items = ItemData.FromItems(World.GetUseItemsList()),
                KeyItems = ItemData.FromItems(World.GetKeyItemsList()),
                PartyMembers = World.Party.ToCharacterInfoList(),
                Quests = QuestData.FromQuests(World.GetQuestList()),
                Areas = CreateAreaList(),
                SceneName = SceneManager.GetActiveScene().name,
                Location = World.Party.Members[0].transform.position
            };
            return new GameStateData(config);
        }

        public void FromGameStateData(GameStateData data)
        {
            World.LoadFromGameStateData(data);
            LoadAreas(data.areas);
        }

        private List<AreaData> CreateAreaList()
        {
            var list = new List<AreaData>();
            foreach (var entry in Areas)
                list.Add(new AreaData
                {
                    Id = entry.Value.Id, 
                    Events = CreateAreaEventData(entry.Value.Events)
                });
            return list;
        }

        public void CompleteEventInArea(Area area, string eventId)
        {
            if (!Areas.ContainsKey(area.Id))
            {
                Areas[area.Id] = area;
            }
            CompleteEventInArea(area.Id, eventId);
        }

        public void CompleteEventInArea(string areaId, string eventId)
        {
            if (!Areas.ContainsKey(areaId))
            {
                LogManager.LogWarn($"Area {areaId} is not present in GameState, cannot complete event {eventId}");
                return;
            }
            var events = Areas[areaId].Events;
            if (!events.ContainsKey(eventId))
            {
                LogManager.LogWarn($"Area {areaId} does not contain event {eventId}");
                return;
            }
            events[eventId] = true;
        }

        public bool IsEventComplete(string areaId, string eventId)
        {
            return Areas.ContainsKey(areaId) && Areas[areaId].Events.ContainsKey(eventId)
                && Areas[areaId].Events[eventId];
        }
        private List<AreaEventData> CreateAreaEventData(Dictionary<string, bool> events)
        {
            var list = new List<AreaEventData>();
            foreach (var entry in events)
                list.Add(new AreaEventData
                {
                    Id = entry.Key,
                    Complete = entry.Value
                });
            return list;
        }

        private void LoadAreas(List<AreaData> areas)
        {
            var gameDataAreas = ServiceManager.Get<GameData>().Areas;
            foreach (var area in areas)
            {
                if (!Areas.ContainsKey(area.Id))
                {
                    if (!gameDataAreas.ContainsKey(area.Id))
                    {
                        LogManager.LogError($"Area {area.Id} not found in GameData Areas");
                        continue;
                    }
                    var _area = gameDataAreas[area.Id];
                    foreach (var evt in area.Events)
                    {
                        if (!_area.Events.ContainsKey(evt.Id))
                        {
                            LogManager.LogError($"Event {evt.Id} not found in Area {area.Id} Events.");
                            continue;
                        }
                        _area.Events[evt.Id] = evt.Complete;
                    }
                    Areas.Add(_area.Id, _area);
                }
            }
        }
    }
}