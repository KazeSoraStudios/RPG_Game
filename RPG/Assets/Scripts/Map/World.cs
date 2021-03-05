using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class World : MonoBehaviour
{
    [SerializeField] public bool Input = false;
    [SerializeField] public int Gold;
    [SerializeField] public float PlayTime = 0;
    [SerializeField] public Party Party;
    [SerializeField] Dictionary<string, Item> Items = new Dictionary<string, Item>();
    [SerializeField] Dictionary<string, Item> KeyItems = new Dictionary<string, Item>();
    //mIcons = Icons:Create(Texture.Find("inventory_icons.png")),       string

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
        DebugAssert.Assert(ServiceManager.Get<GameData>().Items[itemId].Type != ItemType.Key, $"Trying to remove {itemId} from inventory but it is a key item.");
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
}
