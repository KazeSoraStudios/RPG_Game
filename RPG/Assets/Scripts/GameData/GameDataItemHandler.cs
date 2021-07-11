namespace RPG_GameData
{
    public class GameDataItemHandler : GameDataHandler
    {
        public static DictionaryList<string, ItemInfo> ProcessItems(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Items.");
            LogManager.LogDebug($"Processing items for data {data}");
            var items = new DictionaryList<string, ItemInfo>();
            for (int i = 0; i < count; i++)
            {
                var id = data[index++];
                var itemInfo = new ItemInfo()
                {
                    Index = i,
                    Id = id,
                    Type = GetEnum(ItemType.None, data[index++]),
                    Icon = data[index++],
                    UseRestriction = GetEnumArray(UseRestriction.None, data[index++]),
                    Description = data[index++],
                    Stats = new ItemStats(data[index++]),
                    Use = data[index++],
                    Price = GetIntFromCell(data[index++])
                };
                index += columnAdvance;
                items.Add(id, itemInfo);
            }
            LogManager.LogDebug("Processing Gamedata Items finished.");
            return items;
        }
    }
}