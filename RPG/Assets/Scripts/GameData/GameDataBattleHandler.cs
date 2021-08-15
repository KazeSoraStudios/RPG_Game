using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataBattleHandler : GameDataHandler
    {
        public static Dictionary<string, Battle> ProcessBattles(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Battles.");
            LogManager.LogDebug($"Processing Battle for data {data}");
            var battles = new Dictionary<string, Battle>();
            for (int i = 0; i < count; i++)
            {
                var battle = new Battle
                {
                    Id = data[index++],
                    Area = data[index++],
                    Enemies = ParseBattleData(data[index++]),
                    BeforeText = ParseBattleData(data[index++]),
                    AfterText = ParseBattleData(data[index++]),
                    CanFlee = data[index++].Equals("TRUE"),
                    Reward = data[index++]
                };
                battles.Add(battle.Id, battle);
                index += columnAdvance;
            }
            LogManager.LogDebug("Processing Gamedata Battles finished.");
            return battles;
        }

        private static List<string> ParseBattleData(string data)
        {
            return new List<string>(data.Split(';'));
        }
    }
}
