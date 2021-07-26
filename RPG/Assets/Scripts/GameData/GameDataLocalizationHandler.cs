using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataLocalizationHandler : GameDataHandler
    {
        public static Dictionary<string, string> ProcessLocaliation(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Localization.");
            LogManager.LogDebug($"Processing Localization for data {data}");
            var terms = new Dictionary<string, string>();
            for (int i = 0; i < count; i++)
            {
                var key = data[index++];
                var value = data[index++].Replace('~', ',');
                terms.Add(key, value);
                index += columnAdvance;
            }
            LogManager.LogDebug("Processing Gamedata Localization finished.");
            return terms;
        }
    }

}