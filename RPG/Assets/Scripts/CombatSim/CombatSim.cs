using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_CombatSim
{
    public enum PlayerAttackSelector { Random, Weakest }
    public class CombatSim : MonoBehaviour, ICombatEndHandler, ICombatReporter, ITurnHandler
    {
        [Serializable]
        public class ActorData
        {
            public int Level = 1;
            public int HP;
            public int MP;
            public int Attack;
            public int Defense;
            public int Magic;
            public int Resist;
            public int Speed;
            public string Id;
            private Stats stats;

            public Stats Stats()
            {
                stats.SetStat(Stat.HP, HP);
                stats.SetStat(Stat.MP, MP);
                stats.SetStat(Stat.Attack, Attack);
                stats.SetStat(Stat.Defense, Defense);
                stats.SetStat(Stat.Magic, Magic);
                stats.SetStat(Stat.Resist, Resist);
                stats.SetStat(Stat.Speed, Speed);
                return stats;
            }

            public ActorData(string id, Stats stats)
            {
                Id = id;
                this.stats = stats;
                HP = stats.Get(Stat.HP);
                MP = stats.Get(Stat.MP);
                Attack = stats.Get(Stat.Attack);
                Defense = stats.Get(Stat.Defense);
                Magic = stats.Get(Stat.Magic);
                Resist = stats.Get(Stat.Resist);
                Speed = stats.Get(Stat.Speed);
            }
        }

        public bool AlwaysMelee;
        public bool AlwaysMagic;
        public int NumberOfBattles;
        public int TotalBattles;
        public int SimDataToView = 0;
        public PlayerAttackSelector AttackSelector;
        public float AttackLean = 0.5f;
        public string[] Party = new string[3];
        public string[] Enemies = new string[7];
        public TextMeshProUGUI Wins;
        public TextMeshProUGUI Loses;
        public TextMeshProUGUI WinPercent;
        public TextMeshProUGUI Dodges;
        public TextMeshProUGUI Misses;
        public TextMeshProUGUI Hits;
        public TextMeshProUGUI Crits;
        public EventQueue EventQueue;
        public GameDataDownloader Downloader;
        public List<Drop> drops = new List<Drop>();
        public List<ActorData> PartyData = new List<ActorData>();
        public List<ActorData> EnemyData = new List<ActorData>();
        public List<CombatSimData> SimData = new List<CombatSimData>();

        private int currentSimData = 0;
        private Action onWin;
        private Action onDie;
        private StateStack stack;
        private GameData gameData;
        private CombatGameState combat;
        private List<Actor> partyPrefabs = new List<Actor>();
        private List<Actor> enemyPrefabs = new List<Actor>();

        private void Awake()
        {
            LogManager.SetLogLevel(LogLevel.Info);
        }

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
            combat.EndHandler = this;
            combat.TurnHandler = this;
            SimData.Add(new CombatSimData());
        }

        private void Update() 
        {
            stack.Update(Time.deltaTime);
        }

        public void Init(ICombatState state, Action onWin, Action onDie)
        {
            this.onWin = onWin;
            this.onDie = onDie;
        }

        public void OnWin(StateStack stack)
        {
            SimData[currentSimData].TotalWins++;
            OnBattleFinished();
        }

        public void OnLose(StateStack stack)
        {
            SimData[currentSimData].TotalLoses++;
            OnBattleFinished();
        }

        public List<Drop> Drops() => drops;

        public void ReportResult(FormulaResult result, string name)
        {
            SimData[currentSimData].TotalTurns++;
            if (result.Result == CombatFormula.HitResult.Miss)
                SimData[currentSimData].TotalMisses++;
            else if (result.Result == CombatFormula.HitResult.Dodge)
                SimData[currentSimData].TotalDodges++;
            else if (result.Result == CombatFormula.HitResult.Hit)
                {}
            else
                SimData[currentSimData].TotalCrits++;
            DisplayResults();
        }

        public void DisplayInfo(string info)
        {
            LogManager.LogInfo(info);
        }

        public void Reset()
        {
            if (SimDataToView < 0 || SimDataToView >= SimData.Count)
                currentSimData = 0;
            else
                currentSimData = SimDataToView;
            DisplayResults();
        }

        public void Init(ICombatState state)
        {
            EventQueue.Clear();
            AddTurns();
        }

        public void Execute()
        {
            EventQueue.Execute();
            if (EventQueue.IsEmpty())
                AddTurns();
        }

        public void ClearTurns()
        {
            EventQueue.Clear();
        }

        public void AddEvent(IEvent evt, int speed)
        {
            EventQueue.Add(evt, speed);
        }

        public void RemoveEventsForActor(int id)
        {
            EventQueue.RemoveEventsForActor(id);
        }

        private void AddTurns()
        {
            AddPlayerTurns(combat.GetPartyActors());
            AddEnemyTurns(combat.GetEnemyActors());
        }

        private void AddPlayerTurns(List<Actor> actors, bool forceFirst = false)
        {
            foreach (var actor in actors)
            {
                var firstSpeed = Constants.MAX_STAT_VALUE + 1;
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CESimTurn(actor, combat, this);
                    var speed = forceFirst ? firstSpeed : turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding turn for {actor.name}");
                }
            }
        }

        private void AddEnemyTurns(List<Actor> actors)
        {
            foreach (var actor in actors)
            {
                var isAlive = actor.Stats.Get(Stat.HP) > 0;
                if (isAlive && !EventQueue.ActorHasEvent(actor.Id))
                {
                    var turn = new CSEAITurn(actor, combat);
                    var speed = turn.CalculatePriority(EventQueue);
                    EventQueue.Add(turn, speed);
                    LogManager.LogDebug($"Adding AI turn for {actor.name}");
                }
            }
        }

        private void OnBattleFinished()
        {
            stack.Pop();
            TotalBattles++;
            if (TotalBattles >= NumberOfBattles)
            {
                DisplayResults();
            }
            else
            {
                StartCombat();
            }
        }

        private void DisplayResults()
        {
            var current = SimData[currentSimData];
            Wins.SetText($"{current.TotalWins}");
            Loses.SetText($"{current.TotalLoses}");
            float percent =  current.TotalWins + current.TotalLoses == 0 ? 
            current.TotalWins : current.TotalWins / (float)(current.TotalWins + current.TotalLoses);
            WinPercent.SetText($"{percent}");
            Dodges.SetText($"{current.TotalDodges}");
            Misses.SetText($"{current.TotalMisses}");
            Hits.SetText($"{current.TotalTurns - current.TotalDodges - current.TotalMisses}");
            Crits.SetText($"{current.TotalCrits}");
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
                Reporter = this
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
                    var actorData = new ActorData(hero, new Stats(gameData.Stats[hero + "_stats"], name));
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
                    var actorData = new ActorData(enemy, new Stats(gameData.Stats[enemy + "_stats"], name));
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
                    partyPrefabs[i].GoToLevel(PartyData[i].Level, true);
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
                    enemyPrefabs[i].GoToLevel(EnemyData[i].Level, false);
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
                hero.Init(null, Constants.PARTY_STATES, Constants.WAIT_STATE);
                partyPrefabs.Add(hero.GetComponent<Actor>());
            }
            for (int i = 0; i < 7; i++)
            {
                var hero = GameObject.Instantiate(obj);
                hero.gameObject.transform.SetParent(transform, true);
                hero.Init(null, Constants.PARTY_STATES, Constants.WAIT_STATE);
                enemyPrefabs.Add(hero.GetComponent<Actor>());
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
