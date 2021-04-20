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
            public float HpFill;
            public float MpFill;
            public string Hp;
            public string Mp;
        }

        [SerializeField] TextMeshProUGUI HpText;
        [SerializeField] TextMeshProUGUI MpText;
        [SerializeField] ProgressBar HpBar;
        [SerializeField] ProgressBar MpBar;

        public void Init(Config config)
        {
            if (!CheckUIConfigAndLogError(config, "HpMpWidget"))
                return;
            if (HpText != null)
                HpText.SetText(config.Hp);
            if (MpText != null)
                MpText.SetText(config.Mp);
            if (HpBar != null)
                HpBar.SetTargetFillAmountImmediate(config.HpFill);
            if (MpBar != null)
                MpBar.SetTargetFillAmountImmediate(config.MpFill);
        }
    }
}