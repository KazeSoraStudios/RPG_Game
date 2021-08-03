using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPG_Character;
using RPG_Combat;

namespace RPG_UI
{
    public class CombatUI : MonoBehaviour
    {
        [SerializeField] CombatMenuWidget CombatMenu;
        [SerializeField] TextMeshProUGUI TipText;
        [SerializeField] GameObject NoticeContainer;
        [SerializeField] GameObject TipContainer;
        [SerializeField] TextMeshProUGUI NoticeText;
        public CombatBrowseListState ActionBrowseList;
        public CombatTargetState TargetState;
        public CombatChoiceState CombatChoice;

        public void LoadMenuUI(List<Actor> party)
        {
            var menuConfig = new CombatMenuWidget.Config
            {
                Actors = party
            };
            CombatMenu.Init(menuConfig);
        }

        public void ShowTip(string text)
        {
            if (TipContainer == null)
                return;
            TipContainer.SafeSetActive(true);
            TipText.SetText(text);
        }

        public void ShowNotice(string text)
        {
            if (NoticeContainer == null)
                return;
            NoticeContainer.SafeSetActive(true);
            NoticeText.SetText(text);
        }

        public void HideTip()
        {
            if (TipContainer == null)
                return;
            TipContainer.SafeSetActive(false);
        }

        public void HideNotice()
        {
            if (NoticeContainer == null)
                return;
            NoticeContainer.SafeSetActive(false);
        }

        public void UpdateActorHp(int position, Actor actor)
        {
            if (position != -1)
            {
                CombatMenu.UpdateHp(position, actor.Stats.Get(Stat.HP));
            }
        }

        public void UpdateActorMp(int position, Actor actor)
        {
            if (position != -1)
            {
                CombatMenu.UpdateMp(position, actor.Stats.Get(Stat.MP));
            }
        }
    }
}
