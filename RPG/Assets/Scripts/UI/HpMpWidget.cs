using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RPG_UI
{
    public class HpMpWidget : ConfigMonoBehaviour
    {
        public class Config
        {
            public int Hp;
            public int MaxHp;
            public int Mp;
            public int MaxMp;
        }

        [SerializeField] TextMeshProUGUI HpText;
        [SerializeField] TextMeshProUGUI MpText;
        [SerializeField] ProgressBar HpBar;
        [SerializeField] ProgressBar MpBar;

        private Config config;
        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, "HpMpWidget"))
                return;
            this.config = config;
            if (HpText != null)
                HpText.SetText(string.Format(Constants.STAT_FILL_TEXT, config.Hp, config.MaxHp));
            if (MpText != null)
                MpText.SetText(string.Format(Constants.STAT_FILL_TEXT, config.Mp, config.MaxMp));
            if (HpBar != null)
                HpBar.SetTargetFillAmountImmediate(config.Hp / config.MaxHp);
            if (MpBar != null)
                MpBar.SetTargetFillAmountImmediate(config.Mp / config.MaxMp);
        }

        public void UpdateHp(int hp)
        {
            if (HpText != null)
                HpText.SetText(string.Format(Constants.STAT_FILL_TEXT, hp, config.MaxHp));
            if (HpBar != null)
                HpBar.SetTargetFillAmount(hp / (float)config.MaxHp);
        }

        public void UpdateMp(int mp)
        {
            if (MpText != null)
                MpText.SetText(string.Format(Constants.STAT_FILL_TEXT, mp, config.MaxHp));
            if (MpBar != null)
                MpBar.SetTargetFillAmount(mp / config.MaxHp);
        }
    }
}