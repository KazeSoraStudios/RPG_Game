using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;

namespace RPG_CombatSim
{
    public class CombatSimEndHandler : MonoBehaviour, ICombatEndHandler
    {
        [SerializeField] CombatSim Sim;
        public List<Drop> drops = new List<Drop>();
        private Action onWin;
        private Action onDie;

        public void Init(ICombatState state, Action onWin, Action onDie)
        {
            this.onWin = onWin;
            this.onDie = onDie;
        }

        public void OnWin(StateStack stack)
        {
            Sim.GetCurrentData().TotalWins++;
            Sim.OnBattleFinished();
        }

        public void OnLose(StateStack stack)
        {
            Sim.GetCurrentData().TotalLoses++;
            Sim.OnBattleFinished();
        }

        public List<Drop> Drops() => drops;
    }
}
