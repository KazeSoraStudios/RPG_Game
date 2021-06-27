using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

namespace RPG_GameState
{
	public class GameStateManager : MonoBehaviour
	{
		[SerializeField] bool BinarySave;

		string dirpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/RPG/SaveData/";
		[SerializeField] string filepath = "savedata.rpg";
		private GameStateData current;
		private List<GameStateData> savedGames = new List<GameStateData>();

		private void Awake()
		{
			ServiceManager.Register(this);
        }

        private void OnDestroy()
        {
			ServiceManager.Unregister(this);
		}

		public GameStateData GetCurrent()
		{
			return current;
        }

		public List<GameStateData> GetSavedStates()
		{
			return savedGames;
        }

		public int GetNumberOfSaves()
		{
			return savedGames.Count;
        }

        public void SaveGameStateData(GameStateData state)
		{
			LogManager.LogDebug("Starting Save GameState.");
			if (BinarySave)
			{
				if (!Directory.Exists(dirpath))
					Directory.CreateDirectory(dirpath);
				savedGames.Add(state);
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Create(dirpath + filepath);
				bf.Serialize(file, savedGames);
				file.Close();
			}
			else
			{
				if (!Directory.Exists(dirpath))
						Debug.Log("Not found");
				Directory.CreateDirectory(dirpath);
				using (var writer = new StreamWriter(dirpath + filepath))
				{					
					var json = JsonUtility.ToJson(state);
					writer.Write(json);
					writer.Flush();
				}
			}
			LogManager.LogDebug("Finished Saving GameState.");
		}

		public void DeleteSavedGame(int index)
		{
			if (index < 0 || savedGames.Count == 0 || index > savedGames.Count)
			{
				LogManager.LogError($"Index [{index}] is outside range of saved games [{savedGames.Count}]");
				return;
			}
			savedGames.RemoveAt(index);
		}

		public void LoadSavedGames()
		{
			LogManager.LogDebug("Trying to Load GameState.");
			if (!File.Exists(dirpath + filepath))
			{
				LogManager.LogDebug("GameState not found, cannot load.");
				return;
			}
			if (BinarySave)
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(dirpath + filepath, FileMode.Open);
				savedGames = (List<GameStateData>)bf.Deserialize(file);
				file.Close();
			}
			else
			{
				using (var reader = new StreamReader(dirpath + filepath))
				{
					var json = reader.ReadToEnd();
					var game = JsonUtility.FromJson<GameStateData>(json);
					savedGames.Add(game);
				}
			}
			LogManager.LogDebug("Finished Loading GameState.");
		}

		public void LoadGameStateData(int index)
		{
			if (index < 0 || savedGames.Count == 0 ||index > savedGames.Count)
			{
				LogManager.LogError($"Index [{index}] is outside range of saved games [{savedGames.Count}]");
				return;
            }
			current = savedGames[index];
        }

		public GameState LoadGameStateFromCurrentData()
		{
			var game = new GameState();
			if (current == null)
				return game;
			game.FromGameStateData(current);
			return game;
        }
	}

}