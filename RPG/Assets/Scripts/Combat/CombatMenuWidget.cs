using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPG_Character;

namespace RPG_UI
{
    public class CombatMenuWidget : ConfigMonoBehaviour
    {
        public class Config
        {
            public List<Actor> Actors;
        }

        [SerializeField] MenuOptionsList MenuOptions;
        [SerializeField] HpMpWidget[] PartyMemeberStats;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, "CombatMenuWidget"))
                return;
            InitMenuList(config.Actors);
            SetActorStats(config.Actors);
        }

        public void UpdateHp(int position, int hp)
        {
            if (position < 0 || position > PartyMemeberStats.Length)
            {
                LogManager.LogError($"Position [{position}] passed to UpdateHp");
                return;
            }
            PartyMemeberStats[position].UpdateHp(hp);
        }

        public void UpdateMp(int position, int mp)
        {
            if (position < 0 || position > PartyMemeberStats.Length)
            {
                LogManager.LogError($"Position [{position}] passed to UpdateMp");
                return;
            }
            PartyMemeberStats[position].UpdateMp(mp);
        }

        private void InitMenuList(List<Actor> actors)
        {
            if (MenuOptions == null)
            { 
                LogManager.LogError("MenuOptionsList is null in CombatMenuWidget");
                return;
            }
            var names = actors.Select(a => a.Name).ToList();
            var config = new MenuOptionsList.Config { Names = names, ShowSelection = false };
            MenuOptions.Init(config);
        }

        private void SetActorStats(List<Actor> actors)
        {
            int i = 0;
            for (; i < actors.Count && i < PartyMemeberStats.Length; i++)
            {
                var actor = actors[i];
                var hp = actor.Stats.Get(Stat.HP);
                var maxhp = actor.Stats.Get(Stat.MaxHP);
                var mp = actor.Stats.Get(Stat.MP);
                var maxMp = actor.Stats.Get(Stat.MaxMP);
                var config = new HpMpWidget.Config
                {
                    Hp = hp,
                    MaxHp = maxhp,
                    Mp = mp,
                    MaxMp = maxMp,
                };
                PartyMemeberStats[i].Init(config);
            }
            for (; i < PartyMemeberStats.Length; i++)
                PartyMemeberStats[i].gameObject.SafeSetActive(false);
        }
    }
}