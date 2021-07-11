using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG_GameData
{
    public class GameDataShopHandler : GameDataHandler
        {
            public static Dictionary<string, Shop> ProcessShops(int index, int count, int columnAdvance, string[] data)
            {
                LogManager.LogDebug("Creating GameData 'Shops'.");
                LogManager.LogDebug($"Processing Shops for data {data}");
                var shops = new Dictionary<string, Shop>();
                for (int i = 0; i < count; i++)
                {
                    var id = data[index++];
                    var shop = new Shop()
                    {
                        Id = id,
                        Items = GetListFromCell(data[index++]),
                        AdditionalItems = GetAdditionalItems(data[index++])
                    };
                    index += columnAdvance;
                    shops.Add(id, shop);
                }
                LogManager.LogDebug("Processing Gamedata Quests finished.");
                return shops;
            }

            private static List<string> GetListFromCell(string data)
            {
                var list = new List<string>();
                var items = data.Split(':');
                foreach (var item in items)
                    list.Add(item);
                return list;
            }

            private static Dictionary<string, List<string>> GetAdditionalItems(string data)
            {
                var additionalItems = new Dictionary<string, List<string>>();
                if (data.IsEmptyOrWhiteSpace())
                    return additionalItems;
                var events = data.Split(';');
                foreach (var e in events)
                {
                    var info = e.Split(':');
                    if (info.Length < 2)
                        continue;
                    int i = 1;
                    var items = new List<string>();
                    for (; i < info.Length; i++)
                        items.Add(info[i]);
                    additionalItems.Add(info[0], items);
                }
                return additionalItems;
            }
    }
}