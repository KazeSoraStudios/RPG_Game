using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPG_Character;

namespace RPG_UI
{
    public class StatsWidget : ConfigMonoBehaviour
    {
        public struct Config
        {
            public Stats Stats;
        }
        [SerializeField] TextMeshProUGUI StrengthText;
        [SerializeField] TextMeshProUGUI SpeedText;
        [SerializeField] TextMeshProUGUI IntelligenceText;
        [SerializeField] TextMeshProUGUI AttackText;
        [SerializeField] TextMeshProUGUI DefenseText;
        [SerializeField] TextMeshProUGUI MagicText;
        [SerializeField] TextMeshProUGUI ResistText;
        [SerializeField] TextMeshProUGUI HPText;
        [SerializeField] TextMeshProUGUI MPText;

        private Config config;
        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, "StatsWidget"))
                return;
            this.config = config;
            DisplayStats();
        }

        public void DisplayStats()
        {
            var stats = config.Stats;
            if (StrengthText != null)
                StrengthText.SetText(stats.Get(Stat.Strength).ToString());
            if (SpeedText != null)
                SpeedText.SetText(stats.Get(Stat.Speed).ToString());
            if (IntelligenceText != null)
                IntelligenceText.SetText(stats.Get(Stat.Intelligence).ToString());
            if (AttackText != null)
                AttackText.SetText(stats.Get(Stat.Attack).ToString());
            if (DefenseText != null)
                DefenseText.SetText(stats.Get(Stat.Defense).ToString());
            if (MagicText != null)
                MagicText.SetText(stats.Get(Stat.Magic).ToString());
            if (ResistText != null)
                ResistText.SetText(stats.Get(Stat.Resist).ToString());
            if (HPText != null)
                HPText.SetText(stats.Get(Stat.MaxHP).ToString());
            if (MPText != null)
                MPText.SetText(stats.Get(Stat.MaxMP).ToString());
        }

        public void ShowPredictionStats(List<int> predictions)
        {
            // order HP, MaxHP, MP, MaxMP, Strength, Speed, Intelligence, Dodge, Counter, Attack, Defense, Resist, Magic
            var stats = config.Stats;
            foreach (var stat in (Stat[])Enum.GetValues(typeof(Stat)))
            {
                if (stat == Stat.HP || stat == Stat.MP || stat == Stat.Counter || stat == Stat.Dodge)
                    continue;
                int current = stats.Get(stat);
                int prediction = predictions[(int)stat];
                SetPrediction(stat, current, prediction);
            }
        }

        private void SetPrediction(Stat stat, int current, int prediction)
        {
            var index = (int)stat;
            var text = GetPredictionText(stat);
            var format = current > prediction ? Constants.STAT_DIFF_DECREASE_TEXT : Constants.STAT_DIFF_INCREASE_TEXT;
            string displayText = current == prediction ? current.ToString() : string.Format(format, current, prediction);
            text.SetText(displayText);
        }

        private TextMeshProUGUI GetPredictionText(Stat stat)
        {
            switch (stat)
            {
                case Stat.Strength:
                    return StrengthText;
                case Stat.Speed:
                    return SpeedText;
                case Stat.Intelligence:
                    return IntelligenceText;
                case Stat.Attack:
                    return AttackText;
                case Stat.Defense:
                    return DefenseText;
                case Stat.Magic:
                    return MagicText;
                case Stat.Resist:
                    return ResistText;
                case Stat.MaxHP:
                    return HPText;
                case Stat.MaxMP:
                    return MPText;
                default:
                    LogManager.LogError($"Not stat prediction text for stat: {stat}");
                    return null;
            }
        }
    }
}