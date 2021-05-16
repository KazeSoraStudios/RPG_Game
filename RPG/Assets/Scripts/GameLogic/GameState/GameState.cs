using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG_GameState
{
    public class GameState : MonoBehaviour
    {
        public World World;

        // TODO other info for current state of the game

        public void Start()
        {
            World.Reset();
        }

        public GameStateData ToGameStateData()
        {
            var config = new GameStateData.Config
            {
                Gold = World.Gold,
                PlayTime = World.PlayTime,
                Items = ItemData.FromItems(World.GetUseItemsList()),
                KeyItems = ItemData.FromItems(World.GetKeyItemsList()),
                PartyMembers = World.Party.ToCharacterInfoList(),
                Quests = QuestData.FromQuests(World.GetQuestList())
            };
            return new GameStateData(config);
        }

        public void FromGameStateData(GameStateData data)
        {
            World.LoadFromGameStateData(data);
        }
    }
}