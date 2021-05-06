using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG_Character;

namespace RPG_UI
{
    public class ActorXPSummary : ConfigMonoBehaviour
    {
        [SerializeField] Transform PopUpLocation;
        [SerializeField] ProgressBar XpBar;
        [SerializeField] ActorSummaryPanel ActorInfo;

        private int popUpDisplayTime = 1;
        private Actor actor;
        private XPSummaryState parent;
        private List<XPPopUp> popUpList = new List<XPPopUp>();

        public void Init(Actor actor, XPSummaryState parent)
        {
            if (CheckUIConfigAndLogError(actor, "ActorXPSummary"))
                return;
            this.actor = actor;
            this.parent = parent;
            ActorInfo.Init(new ActorSummaryPanel.Config { Actor = actor, ShowExp = true });
            gameObject.SafeSetActive(true);
        }

        public void Execute(float deltaTime)
        {
            var fill = actor.Exp / (float)actor.NextLevelExp;
            XpBar.SetTargetFillAmountImmediate(fill);

            if (popUpList.Count < 1)
                return;
            var popup = popUpList[0];
            if (popup.IsFinished())
            {
                LogManager.LogDebug("Removing popup.");
                popUpList.RemoveAt(0);
                parent.ReturnPopUpToPool(popup);
                return;
            }
            popup.Execute(deltaTime);
            if (popup.DisplayTime > popUpDisplayTime && popUpList.Count > 1)
                popup.TurnOff();
        }

        public bool HasPopUps()
        {
            return popUpList.Count > 0;
        }

        public void AddPopUp(string text, Color color)
        {
            XPPopUp popup;
            if (parent.HasPopUpInPool())
                popup = parent.GetPopUpFromPool();
            else
            {
                var asset = ServiceManager.Get<AssetManager>().Load<XPPopUp>(Constants.XP_POPUP_PREFAB);
                popup = Instantiate(asset);
                popup.transform.SetParent(PopUpLocation, false);
            }
            popup.Init(text, color);
            popUpList.Add(popup);
            popup.TurnOn();
        }

        public void CancelPopUp()
        {
            if (popUpList.Count < 1)
            {
                LogManager.LogDebug("Tried to Cancel popup but the list is empty.");
                return;
            }
            var popup = popUpList[0];
            if (popup.IsTurningOff())
                return;
            popup.TurnOff();
        }
    }
}