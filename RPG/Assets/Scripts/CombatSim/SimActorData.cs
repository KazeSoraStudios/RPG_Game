using System;
using RPG_Character;

namespace RPG_CombatSim
{
    [Serializable]
    public class SimActorData
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

        public SimActorData(string id, Stats stats)
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
}