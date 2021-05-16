using System;
using System.Collections.Generic;
using System.Text;
using RPG_Character;
using RPG_GameData;

namespace RPG_GameState
{
    [Serializable]
    public class CharacterInfo
    {
        public int exp;
        public string characterId;
        public StatsData stats;
        public ItemInfo[] equipment = new ItemInfo[3];

        public CharacterInfo() { }

        public CharacterInfo(int exp, string id, StatsData stats, ItemInfo[] equipment) 
        {
            this.exp = exp;
            this.characterId = id;
            this.stats = stats;
            this.equipment = equipment;
        }
    }

    [Serializable]
    public class StatsData
    {
        public List<StatInfo> Stats;
    }

    [Serializable]
    public class StatInfo
    {
        public Stat Stat;
        public int Value;
    }
}