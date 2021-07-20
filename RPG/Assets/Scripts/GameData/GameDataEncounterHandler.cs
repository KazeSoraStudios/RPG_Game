using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataEncounterHandler : GameDataHandler
    {
        public static Dictionary<string, Encounter> ProcessEncounters(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Encounters.");
            LogManager.LogDebug($"Processing Encounters for data {data}");
            var encounters = new Dictionary<string, Encounter>();
            for (int i = 0; i < count; i++)
            {
                var encounter = new Encounter()
                {
                    Id = data[index++],
                    Encounters = GetEncounters(data[index++], data[index++]),
                    Backgrounds = GetBackgrounds(data[index++])
                };
                index += columnAdvance;
                encounters.Add(encounter.Id, encounter);
            }
            LogManager.LogDebug("Processing Gamedata Encounters finished.");
            return encounters;
        }

        private static List<OddmentTable> GetEncounters(string chances, string enemies)
        {
            var tables = new List<OddmentTable>();
            if (chances.IsEmptyOrWhiteSpace() || enemies.IsEmptyOrWhiteSpace())
                return tables;
            var _chances = chances.Split(':');
            var _enemies = enemies.Split(';');
            if (_chances.Length != _enemies.Length)
                return tables;
            for (int i = 0; i < _chances.Length; i++)
            {
                var oddments = new List<Oddment>();
                oddments.Add(new Oddment { Chance = Constants.DEFAULT_ENCOUNTER_CHANCE });
                if (!int.TryParse(_chances[i], out var chance))
                {
                    LogManager.LogError($"Chance {_chances[i]} is not a number.");
                    continue;
                }
                var encounterEnemies = new List<string>(_enemies[i].Split(':'));
                oddments.Add(new Oddment { Chance = chance, Items = encounterEnemies });
                var table = new OddmentTable();
                table.SetOddments(oddments);
                tables.Add(table);
            }
            return tables;
        }

        private static List<string> GetBackgrounds(string data)
        {
            var backgrounds = new List<string>();
            if (data.IsEmptyOrWhiteSpace())
                return backgrounds;
            backgrounds.AddRange(data.Split(':'));
            return backgrounds;
        }
    }
}