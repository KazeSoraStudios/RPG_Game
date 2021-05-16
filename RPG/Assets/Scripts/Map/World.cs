using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPG_Character;
using RPG_GameData;
using RPG_Combat;
using RPG_GameState;

public class World : MonoBehaviour
{
    [SerializeField] public bool Input = false;
    [SerializeField] public int Gold;
    [SerializeField] public float PlayTime = 0;
    [SerializeField] public Party Party;
    [SerializeField] public Transform PersistentCharacters;
    Dictionary<string, Item> Items = new Dictionary<string, Item>();
    Dictionary<string, Item> KeyItems = new Dictionary<string, Item>();
    Dictionary<string, Quest> Quests = new Dictionary<string, Quest>();

    private void Awake()
    {
        ServiceManager.Register(this);
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    public void Reset()
    {
        Input = false;
        Gold = 0;
        PlayTime = 0.0f;
        Items.Clear();
        KeyItems.Clear();
        Party.Reset();
    }

    public void Execute(float deltaTime)
    {
        PlayTime += deltaTime;
    }

    public bool IsInputLocked()
    {
        return Input;
    }

    public void LockInput()
    {
        Input = true;
    }

    public void UnlockInput()
    {
        Input = false;
    }

    public string TimeAsString()
    {
        var time = PlayTime;
        var hours = Mathf.Floor(time / 3600);
        var minutes = Mathf.Floor((time % 3600) / 60);
        var seconds = (int)time % 60;

        return $"{hours}:{minutes}:{seconds}";
    }

    public void AddItem(Item item)
    {
        AddItem(item.ItemInfo, item.Count);
    }

    public void AddItem(ItemInfo itemInfo, int count = 1)
    {
        if (count < 1)
            return;

        if (Items.ContainsKey(itemInfo.Id))
        {
            var item = Items[itemInfo.Id];
            item.Count += count;
            Items[itemInfo.Id] = item;
        }
        else
        {
            var item = new Item(itemInfo, count);
            Items.Add(itemInfo.Id, item);
        }
    }

    public int GetItemCount(string itemId)
    {
        return Items.ContainsKey(itemId) ? Items[itemId].Count : 0;
    }


    public void RemoveItem(string itemId, int amount = 1)
    {
        DebugAssert.Assert(((ItemInfo)ServiceManager.Get<GameData>().Items[itemId]).Type != ItemType.Key, $"Trying to remove {itemId} from inventory but it is a key item.");
        DebugAssert.Assert(amount > 0 && Items.ContainsKey(itemId), $"Trying to remove {itemId} from inventory but it is not in the inventory.");
        var item = Items[itemId];
        item.Count -= amount;
        LogManager.LogDebug($"Removing {amount} of item [{item.ItemInfo.Name}] from inventory. Remaining is: {item.Count}");
        if (item.Count < 1)
            Items.Remove(itemId);
    }

    public List<Item> FilterItems(IComparer<Item> comparer)
    {
        var items = new List<Item>();
        foreach (Item item in Items.Values)
            items.Add(item);
        items.Sort(comparer);
        return items;
    }

    public void AddLoot(List<Item> loot)
    {
        foreach(var item in loot)
        {
            var id = item.ItemInfo.Id;
            if (!Items.ContainsKey(id))
            {
                Items.Add(id, item);
            }
            else
            {
                var currentItem = Items[id];
                currentItem.Count += item.Count;
                Items[id] = currentItem;
            }
        }
    }

    public bool HasItem(string itemId)
    {
        return Items.ContainsKey(itemId);
    }

    public bool HasItemAmount(string itemId, int count = 1)
    {
        if (!HasItem(itemId))
            return false;
        return Items[itemId].Count >= count;
    }

    public bool HasKeyItem(string itemId)
    {
        return KeyItems.ContainsKey(itemId);
    }

    public void AddKeyItem(ItemInfo itemInfo)
    {
        if (KeyItems.ContainsKey(itemInfo.Id))
            return;
        else
        {
            var item = new Item(itemInfo, 1);
            Items.Add(itemInfo.Id, item);
        }
    }

    public void RemoveKeyItem(string itemId)
    {
        if (!Items.ContainsKey(itemId))
            return;
        KeyItems.Remove(itemId);
    }

    public List<Item> GetUseItemsList()
    {
        return Items.Values.ToList<Item>();
    }

    public List<Item> GetKeyItemsList()
    {
        return KeyItems.Values.ToList<Item>();
    }

    public List<Item> GetUseItemsByType(ItemType type)
    {
        var items = new List<Item>();
        foreach (var item in Items)
            if (item.Value.ItemInfo.Type == type)
                items.Add(item.Value);
        return items;
    }

    public void GiveReward(LootData loot)
    {
        if (loot == null)
        {
            LogManager.LogError("Null LootData passed to GiveReward.");
            return;
        }
        Gold += loot.Gold;
        Party.GiveExp(loot.Exp);
        foreach (var item in loot.Loot)
        {
            if (item.ItemInfo.Type == ItemType.Key)
                AddKeyItem(item.ItemInfo);
            else
                AddItem(item);
        }
    }

    public bool HasQuest(string id)
    {
        return Quests.ContainsKey(id);
    }

    public void AddQuest(Quest quest)
    {
        if (Quests.ContainsKey(quest.Id))
        {
            LogManager.LogError($"World already contains Quest: ${quest.Id}.");
            return;
        }
        quest.IsStarted = true;
        Quests.Add(quest.Id, quest);
    }

    public void CompleteQuest(Quest quest)
    {
        if (!Quests.ContainsKey(quest.Id))
        {
            LogManager.LogError($"World does not contain Quest [${quest.Id}], cannot complete.");
            return;
        }
        var reward = quest.CreateReward();
        GiveReward(reward);
    }

    public List<Quest> GetQuestList()
    {
        return Quests.Values.ToList<Quest>();
    }

    public void LoadFromGameStateData(GameStateData data)
    {
        Items.Clear();
        KeyItems.Clear();
        Quests.Clear();
        Gold = data.gold;
        PlayTime = data.playTime;
        LoadItemsFromSaveData(data.items, false);
        LoadItemsFromSaveData(data.keyItems, true);
        LoadQuestsFromSaveData(data.quests);
    }

    private void LoadItemsFromSaveData(List<ItemData> itemData, bool key)
    {
        if (key)
            KeyItems.Clear();
        else
            Items.Clear();
        var items = ServiceManager.Get<GameData>().Items;
        foreach (var item in itemData)
        {
            if (!items.Contains(item.Id))
            {
                LogManager.LogError($"ItemData Id [{item.Id}] not found in GameData Items.");
                continue;
            }
            if (key)
                AddKeyItem(items[item.Id]);
            else
                AddItem(new Item { ItemInfo = items[item.Id], Count = item.Count });
        }
    }

    private void LoadQuestsFromSaveData(List<QuestData> questData)
    {
        Quests.Clear();
        var quests = ServiceManager.Get<GameData>().Quests;
        foreach (var quest in questData)
        {
            if (!quests.ContainsKey(quest.Id))
            {
                LogManager.LogError($"QuestData Id [{quest.Id}] not found in GameData Quests.");
                continue;
            }
            var q = quests[quest.Id];
            q.LoadFromQuestData(quest);
            AddQuest(q);
        }
    }
}
