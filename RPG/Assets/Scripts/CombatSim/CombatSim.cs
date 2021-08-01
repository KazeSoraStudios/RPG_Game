using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_AI;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_CombatSim
{
    public class CombatSim : MonoBehaviour
    {
        public bool AlwaysMelee;
        public bool AlwaysMagic;
        public int NumberOfBattles;
        public int TotalBattles;
        public int TotalTurns;
        public int TotalMisses;
        public float AttackLean = 0.5f;

        private ICombatState state;
        private Action onWin;
        private Action onDie;
        private List<Drop> drops = new List<Drop>();

        void Init(ICombatState state, Action onWin, Action onDie)
        {
            this.state = state;
            this.onWin = onWin;
            this.onDie = onDie;
        }

        void OnWin(StateStack stack)
        {

        }

        void OnLose(StateStack stack)
        {

        }

        List<Drop> Drops() => drops;

    }
}
