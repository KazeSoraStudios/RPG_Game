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

        [SerializeField] TextMeshProUGUI StrengthPredictionText;
        [SerializeField] TextMeshProUGUI SpeedPredictionText;
        [SerializeField] TextMeshProUGUI IntelligencePredictionText;
        [SerializeField] TextMeshProUGUI AttackPredictionText;
        [SerializeField] TextMeshProUGUI DefensePredictionText;
        [SerializeField] TextMeshProUGUI MagicPredictionText;
        [SerializeField] TextMeshProUGUI ResistPredictionText;
        [SerializeField] TextMeshProUGUI HPPredictionText;
        [SerializeField] TextMeshProUGUI MPPredictionText;

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
            TurnOffPredicitionStats();
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
                SetPrediction(stat, prediction);
            }
        }

        public void TurnOffPredicitionStats()
        {
            if (StrengthPredictionText != null)
                StrengthPredictionText.gameObject.SafeSetActive(false);
            if (SpeedPredictionText != null)
                SpeedPredictionText.gameObject.SafeSetActive(false);
            if (IntelligencePredictionText != null)
                IntelligencePredictionText.gameObject.SafeSetActive(false);
            if (AttackPredictionText != null)
                AttackPredictionText.gameObject.SafeSetActive(false);
            if (DefensePredictionText != null)
                DefensePredictionText.gameObject.SafeSetActive(false);
            if (MagicPredictionText != null)
                MagicPredictionText.gameObject.SafeSetActive(false);
            if (ResistPredictionText != null)
                ResistPredictionText.gameObject.SafeSetActive(false);
            if (HPPredictionText != null)
                HPPredictionText.gameObject.SafeSetActive(false);
            if (MPPredictionText != null)
                MPPredictionText.gameObject.SafeSetActive(false);
        }

        private void SetPrediction(Stat stat, int difference)
        {
            var index = (int)stat;
            var text = GetPredictionText(stat);
            string displayText = string.Empty;
            Color color = Color.white;
            bool display = false;
            if (difference < 0)
            {
                display = true;
                color = Color.red;
                displayText = $"{difference * -1}";
            }
            else if (difference > 0)
            {
                display = true;
                color = Color.green;
                displayText = $"{difference}";
            }
            text.gameObject.SafeSetActive(display);
            text.SetText(displayText);
            text.color = color;
        }

        private TextMeshProUGUI GetPredictionText(Stat stat)
        {
            switch (stat)
            {
                case Stat.Strength:
                    return StrengthPredictionText;
                case Stat.Speed:
                    return SpeedPredictionText;
                case Stat.Intelligence:
                    return IntelligencePredictionText;
                case Stat.Attack:
                    return AttackPredictionText;
                case Stat.Defense:
                    return DefensePredictionText;
                case Stat.Magic:
                    return MagicPredictionText;
                case Stat.Resist:
                    return ResistPredictionText;
                case Stat.MaxHP:
                    return HPPredictionText;
                case Stat.MaxMP:
                    return MPPredictionText;
                default:
                    LogManager.LogError($"Not stat prediction text for stat: {stat}");
                    return null;
            }
        }
    }
}