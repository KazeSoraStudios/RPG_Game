using UnityEngine;
using TMPro;
using RPG_Combat;

namespace RPG_CombatSim
{
    public class CombatSimReporter : MonoBehaviour, ICombatReporter
    {
        [SerializeField] TextMeshProUGUI Wins;
        [SerializeField] TextMeshProUGUI Loses;
        [SerializeField] TextMeshProUGUI WinPercent;
        [SerializeField] TextMeshProUGUI Dodges;
        [SerializeField] TextMeshProUGUI Misses;
        [SerializeField] TextMeshProUGUI Hits;
        [SerializeField] TextMeshProUGUI Crits;

        private CombatSim sim;

        public void Init(CombatSim sim)
        {
            this.sim = sim;
        }

        public void ReportResult(FormulaResult result, string name)
        {
            var current = sim.GetCurrentData();
            current.TotalTurns++;
            if (result.Result == CombatFormula.HitResult.Miss)
                current.TotalMisses++;
            else if (result.Result == CombatFormula.HitResult.Dodge)
                current.TotalDodges++;
            else if (result.Result == CombatFormula.HitResult.Hit)
                {}
            else
                current.TotalCrits++;
            DisplayResults();
        }

        public void DisplayInfo(string info)
        {
            LogManager.LogInfo(info);
        }

        public void DisplayResults()
        {
            var current = sim.GetCurrentData();
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
    }
}
