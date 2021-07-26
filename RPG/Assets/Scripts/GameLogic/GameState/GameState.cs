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
        }

        private List<Area> CreateAreaList()
        {
            var list = new List<Area>();
            foreach (var entry in Areas)
                list.Add(entry.Value);
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
        
    }
}