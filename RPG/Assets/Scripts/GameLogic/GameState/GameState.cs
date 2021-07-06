using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_GameData;

namespace RPG_GameState
{
    public class GameState
    {
        public World World;

        public List<Area> Areas = new List<Area>();

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
                Areas = Areas,
                SceneName = SceneManager.GetActiveScene().name,
                Location = World.Party.Members[0].transform.position
            };
            return new GameStateData(config);
        }

        public void FromGameStateData(GameStateData data)
        {
            World.LoadFromGameStateData(data);
        }
    }
}