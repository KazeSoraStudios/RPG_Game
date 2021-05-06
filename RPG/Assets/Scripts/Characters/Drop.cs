using System.Collections.Generic;
using UnityEngine;

namespace RPG_Character
{
    public class Drop
    {
        public int Exp;
        public int Gold;
        public OddmentTable ChanceDrop;
        public List<Item> AlwaysDrop = new List<Item>();

        public Item PickChanceItem()
        {
            var oddment =  ChanceDrop.Pick();
            if (oddment == null)
                return null;
            var item = ServiceManager.Get<GameData>().Items[oddment.Item] as ItemInfo;
            return new Item { Count = oddment.Count, ItemInfo = item };
        }
    }
}