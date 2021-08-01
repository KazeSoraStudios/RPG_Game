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
            public string ActorId = string.Empty;
        }

        public struct UpdateData
        {
            public int Value;
            public string Id;
        }

        [SerializeField] TextMeshProUGUI HpText;
        [SerializeField] TextMeshProUGUI MpText;
        [SerializeField] ProgressBar HpBar;
        [SerializeField] ProgressBar MpBar;

        private Config config;

        private void Awake() 
        {
            GameEventsManager.Register(GameEventConstants.ON_MP_CHANGED, UpdateMp);
        }

        private void OnDestroy() 
        {
            GameEventsManager.Unregister(GameEventConstants.ON_MP_CHANGED, UpdateMp);
        }

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
                HpBar.SetTargetFillAmountImmediate(config.Hp / (float)config.MaxHp);
            if (MpBar != null)
                MpBar.SetTargetFillAmountImmediate(config.Mp / (float)config.MaxMp);
        }

        public void UpdateHp(int hp)
        {
            if (HpText != null)
                HpText.SetText(string.Format(Constants.STAT_FILL_TEXT, hp, config.MaxHp));
            if (HpBar != null)
                HpBar.SetTargetFillAmount(hp / (float)config.MaxHp);
        }

        public void UpdateMp(object obj)
        {
            if (!gameObject.activeSelf)
                return;
            if (obj == null || !(obj is UpdateData))
            {
                LogManager.LogError("Null object passed to UpdateMP.");
                return;
            }
            var data = (UpdateData)obj;
            if (!data.Id.Equals(config.ActorId))
                return;
            int mp = data.Value;
            if (MpText != null)
                MpText.SetText(string.Format(Constants.STAT_FILL_TEXT, mp, config.MaxMp));
            if (MpBar != null)
                MpBar.SetTargetFillAmount(mp / config.MaxHp);
        }
    }
}