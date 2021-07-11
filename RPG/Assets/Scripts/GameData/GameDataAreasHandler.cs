using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataAreasHandler : GameDataHandler
    {
        public static Dictionary<string, Area> ProcessAreas(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Areas.");
            LogManager.LogDebug($"Processing Areas for data {data}");
            var areas = new Dictionary<string, Area>();
            for (int i = 0; i < count; i++)
            {
                var area = new Area()
                {
                    Id = data[index++],
                    Events = BuildDictionary(data[index++]),
                    Chests = BuildDictionary(data[index++]),
                    Items = BuildDictionary(data[index++])
                };
                index += columnAdvance;
                areas.Add(area.Id, area);
            }
            LogManager.LogDebug("Processing Gamedata Areas finished.");
            return areas;
        }

        private static Dictionary<string, bool> BuildDictionary(string listCell)
        {
            var dictionary = new Dictionary<string, bool>();
            if (listCell.IsEmptyOrWhiteSpace())
                return dictionary;
            var data = listCell.Split(':');
            foreach (var d in data)
                dictionary.Add(d, false);
            return dictionary;
        }
    }
}