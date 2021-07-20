using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityGoogleDrive;
using RPG_Audio;

namespace RPG_GameData
{
    public class GameDataDownloader : MonoBehaviour
    {
        [SerializeField] bool DownloadGameData;
        [SerializeField] GameData GameData;

        private const string sheetID = "1bmAVofqQuVjT_QvqyrGK6_vwzcdfymHfTYjUSSrgjQY";
        private readonly string gameDataPath = "/Resources/GameData/gamedata.txt";
        private Action OnComplete;

        public void LoadGameData(Action callback = null)
        {
            OnComplete = callback;
            if (DownloadGameData)
                DownLoadGameData();
            else
                LoadFromSavedFile();
        }

        private void DownLoadGameData()
        {
            GoogleDriveFiles.Export(sheetID, "text/csv").Send().OnDone += OnLoad;
        }

        private void LoadFromSavedFile()
        {
            var data = File.ReadAllText(Application.dataPath + gameDataPath);
            var cells = data.Split(',');
            HandleData(cells);
        }

        public void OnLoad(UnityGoogleDrive.Data.File file)
        {
            var content = System.Text.Encoding.Default.GetString(file.Content);
            // Remove new lines
            content = Regex.Replace(content, @"\r\n?|\n", ",");
            var cells = content.Split(',');
            HandleData(cells);
            File.WriteAllText(Application.dataPath + gameDataPath, content);
        }

        private void HandleData(string[] data)
        {
            // First row is total number of rows and columns
            var cells = new Dictionary<string, List<List<string>>>();
            int maxColumns = int.Parse(data[1]);
            int index = maxColumns;
            // Second row is sheet name, number of rows and columns
            int numberOfCells = int.Parse(data[index + 1]);
            int numberOfColumns = int.Parse(data[index + 2]);
            int cellsProcessed = 0;
            index = maxColumns * 2;
            var items = GameDataItemHandler.ProcessItems(index, numberOfCells, maxColumns - numberOfColumns, data);
            // Count the cells we just processed plus the two rows we skipped
            cellsProcessed = maxColumns * 2 + maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var itemUses = GameDataItemUseHandler.ProcessItems(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var loc = GameDataLocalizationHandler.ProcessLocaliation(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var party = GameDataPartyHandler.ProcessParty(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var stats = GameDataStatsHandler.ProcessStats(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var spells = GameDataSpellHandler.ProcessSpells(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var enemies = GameDataEnemyHandler.PrcoessEnemies(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var quests = GameDataQuestHandler.ProcessQuests(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var areas = GameDataAreasHandler.ProcessAreas(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var shops = GameDataShopHandler.ProcessShops(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var encounters = GameDataEncounterHandler.ProcessEncounters(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var aiData = EnemyAIGameDataHandler.PrcoessEnemyAI(index, numberOfCells, maxColumns - numberOfColumns, data);
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data[cellsProcessed + 1]);
            numberOfColumns = int.Parse(data[cellsProcessed + 2]);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
            var audio = GameDataAudioHandler.ProcessAudio(index, numberOfCells, maxColumns - numberOfColumns, data);

            GameData.Items = items;
            GameData.ItemUses = itemUses;
            GameData.PartyDefs = party;
            GameData.Stats = stats;
            GameData.Spells = spells.spells;
            GameData.Specials = spells.specials;
            GameData.Enemies = enemies;
            GameData.Quests = quests;
            GameData.Areas = areas;
            GameData.Shops = shops;
            GameData.Encounters = encounters;
            GameData.EnemyAI = aiData;
            ServiceManager.Get<LocalizationManager>().SetLocalization(loc);
            ServiceManager.Get<AudioManager>().LoadLibrary(audio);
            enabled = false;
            OnComplete?.Invoke();
        }

        private void AdvanceValues(ref int cellsProcessed, ref int maxColumns, ref int numberOfCells, ref int numberOfColumns, ref int index, string data1, string data2)
        {
            cellsProcessed += maxColumns * numberOfCells;
            // Get the new number of cells to process and skip the row
            numberOfCells = int.Parse(data1);
            numberOfColumns = int.Parse(data2);
            cellsProcessed += maxColumns;
            index = cellsProcessed;
        }
    }
}