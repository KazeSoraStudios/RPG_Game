using System;
using UnityEngine;
using RPG_Character;

namespace RPG_UI
{
    public class CharacterShopWidgetList : MonoBehaviour
    {
        [SerializeField] CharacterShopWidget[] CharacterWidgets;

        private int widgetCount;
        public void Init()
        {
            var party = ServiceManager.Get<Party>().Members;
            var characterLength = CharacterWidgets.Length;
            var partyMembers = party.Count;
            int i = 0;
            for (; i < characterLength && i < partyMembers; i++)
            {
                CharacterWidgets[i].gameObject.SafeSetActive(true);
                CharacterWidgets[i].Init(party[i]);
            }
            widgetCount = i;
            for (; i < characterLength; i++)
                CharacterWidgets[i].gameObject.SafeSetActive(false);
        }

        public void UpdateForItem(ItemInfo item)
        {
            for (int i = 0; i < widgetCount; i++)
                CharacterWidgets[i].UpdateForItem(item);
        }

        public void UpdateEquipOnly(ItemInfo item)
        {
            for (int i = 0; i < widgetCount; i++)
                CharacterWidgets[i].UpdateEquipOnly(item);
        }

        public void ResetWidgets()
        {
            for (int i = 0; i < widgetCount; i++)
                CharacterWidgets[i].ResetWidget();
        }
    }
}
