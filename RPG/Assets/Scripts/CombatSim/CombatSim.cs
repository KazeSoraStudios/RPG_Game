using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_CombatSim
{
    public enum PlayerAttackSelector { Random, Weakest }
    public class CombatSim : MonoBehaviour
    {
        [SerializeField] public bool AlwaysMelee;
        [SerializeField] public bool AlwaysMagic;
        [SerializeField] int NumberOfBattles;
        [SerializeField] int TotalBattles;
        [SerializeField] int SimDataToView = 0;
        [SerializeField] public PlayerAttackSelector AttackSelector;
        [SerializeField] public float AttackLean = 0.5f;
        [SerializeField] string[] Party = new string[3];
        [SerializeField] string[] Enemies = new string[7];
        [SerializeField] List<SimActorData> PartyData = new List<SimActorData>();
        [SerializeField] List<SimActorData> EnemyData = new List<SimActorData>();
        [SerializeField] List<CombatSimData> SimData = new List<CombatSimData>();
        [SerializeField] GameDataDownloader Downloader;
        [SerializeField] CombatSimTurnHandler TurnHandler;
        [SerializeField] CombatSimReporter Reporter;
        [SerializeField] CombatSimEndHandler EndHandler;

        private int currentSimData = 0;
        private StateStack stack;
        private GameData gameData;
        private CombatGameState combat;
        private List<Actor> partyPrefabs = new List<Actor>();
        private List<Actor> enemyPrefabs = new List<Actor>();

        private void Start() 
        {
            Downloader.LoadGameData();
            LoadPrefabs();
            stack = new StateStack();
            stack.SuppressErrorState();
            gameData = ServiceManager.Get<GameData>();
            var asset = ServiceManager.Get<AssetManager>().Load<CombatGameState>(Constants.COMBAT_PREFAB_PATH);
            if (asset == null)
            {
                LogManager.LogError($"Unable to load Combat: {Constants.COMBAT_PREFAB_PATH}");
                return;
            }
            combat = GameObject.Instantiate(asset, Vector3.zero, Quaternion.identity);
            SimData.Add(new CombatSimData());
            Reporter.Init(this);
            LogManager.SetLogLevel(LogLevel.Debug);
        }

        private void Update() 
        {
            stack.Update(Time.deltaTime);
        }

        public CombatSimData GetCurrentData()
        {
            return SimData[currentSimData];
        }

        public void Reset()
        {
            if (SimDataToView < 0 || SimDataToView >= SimData.Count)
                currentSimData = 0;
            else
                currentSimData = SimDataToView;
            Reporter.DisplayResults();
        }

        public void OnBattleFinished()
        {
            stack.Pop();
            TotalBattles++;
            if (TotalBattles >= NumberOfBattles)
            {
                Reporter.DisplayResults();
            }
            else
            {
                StartCombat();
            }
        }

        public void RunBattle()
        {
            SimData.Add(new CombatSimData());
            currentSimData = SimData.Count - 1;
            TotalBattles = 0;
            StartCombat();
        }

        private void StartCombat()
        {
            var combatConfig = new CombatGameState.Config
            {
                CanFlee = true,
                Sim = true,
                Party = CreateParty(),
                Enemies = CreateEnemies(),
                Stack = stack,
                Reporter = Reporter,
                TurnHandler = TurnHandler,
                EndHandler = EndHandler
            };
            combat.Init(combatConfig);
            stack.Push(combat);
        }

        public void CreatePartyData()
        {
            PartyData.Clear();
            var data = gameData.PartyDefs;
            foreach (var hero in Party)
            {
                if (hero.IsEmptyOrWhiteSpace())
                    continue;
                if (!data.ContainsKey(hero))
                {
                    LogManager.LogError($"{hero} not in GameData.");
                    continue;
                }
                if (GetPartyDataIndex(hero) == -1)
                {    
                    var actorData = new SimActorData(hero, new Stats(gameData.Stats[hero + "_stats"], name));
                    PartyData.Add(actorData);
                }
            }
        }

        public void CreateEnemyData()
        {
            EnemyData.Clear();
            var data = gameData.Enemies;
            foreach (var enemy in Enemies)
            {
                if (enemy.IsEmptyOrWhiteSpace())
                    continue;
                if (!data.ContainsKey(enemy))
                {
                    LogManager.LogError($"{enemy} not in GameData.");
                    continue;
                }
                if (GetEnemyDataIndex(enemy) == -1)
                {
                    var actorData = new SimActorData(enemy, new Stats(gameData.Stats[enemy + "_stats"], name));
                    EnemyData.Add(actorData);
                }
            }
        }

        private List<Actor> CreateParty()
        {
            var party = new List<Actor>();
            var data = gameData.PartyDefs;
            for (int i = 0; i < Party.Length; i++)
            {
                var hero = Party[i];
                if (hero.IsEmptyOrWhiteSpace())
                    continue;
                if (!data.ContainsKey(hero))
                {
                    LogManager.LogError($"{hero} not in GameData.");
                    continue;
                }
                int index = GetPartyDataIndex(hero);
                partyPrefabs[i].Init(data[hero]);
                if (index != -1)
                {
                    partyPrefabs[i].Stats = PartyData[i].Stats();
                }
                party.Add(partyPrefabs[i]);
            }
            return party;
        }

        private List<Actor> CreateEnemies()
        {
            var enemies = new List<Actor>();
            var data = gameData.Enemies;
            for (int i = 0; i < Enemies.Length; i++)
            {
                var enemy = Enemies[i];
                if (enemy.IsEmptyOrWhiteSpace())
                    continue;
                if (!data.ContainsKey(enemy))
                {
                    LogManager.LogError($"{enemy} not in GameData.");
                    continue;
                }
                int index = GetEnemyDataIndex(enemy);
                enemyPrefabs[i].Init(data[enemy]);
                if (index != -1)
                {
                    enemyPrefabs[i].Stats = EnemyData[i].Stats();
                }
                enemies.Add(enemyPrefabs[i]);
            }
            return enemies;
        }

        private void LoadPrefabs()
        {
            var obj = ServiceManager.Get<AssetManager>().Load<Character>(Constants.HERO_PREFAB);
            if (obj == null)
            {
                LogManager.LogError("Unable to load hero in ExplroeState.");
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                var hero = GameObject.Instantiate(obj);
                hero.gameObject.transform.SetParent(transform, true);
                hero.Init(Constants.PARTY_STATES, Constants.WAIT_STATE);
                hero.name = $"Party{i}";
                partyPrefabs.Add(hero.GetComponent<Actor>());
            }
            for (int i = 0; i < 7; i++)
            {
                var enemy = GameObject.Instantiate(obj);
                enemy.gameObject.transform.SetParent(transform, true);
                enemy.Init(Constants.PARTY_STATES, Constants.WAIT_STATE);
                enemy.name = $"Enemy{i}";
                enemyPrefabs.Add(enemy.GetComponent<Actor>());
            }
        }

        private int GetPartyDataIndex(string id)
        {
            for (int i = 0; i < PartyData.Count; i++)
                if (PartyData[i].Id.Equals(id))
                    return i;
            return -1;
        }

        private int GetEnemyDataIndex(string id)
        {
            for (int i = 0; i < EnemyData.Count; i++)
                if (EnemyData[i].Id.Equals(id))
                    return i;
            return -1;
        }
    }
}
