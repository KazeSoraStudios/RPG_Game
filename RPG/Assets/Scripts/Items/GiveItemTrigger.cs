using UnityEngine;
using RPG_GameData;

public class GiveItemTrigger : MonoBehaviour, Trigger
{
    [SerializeField] int Amount = 1;
    [SerializeField] string ItemId;

    private bool gaveItem;
    private Item item;

    void Start()
    {
        if (ItemId.IsEmptyOrWhiteSpace())
            return;
        var items = ServiceManager.Get<GameData>().Items;
        if (!items.Contains(ItemId))
        {
            LogManager.LogError($"ItemId [{ItemId}] not found in GameData Items.");
            return;
        }
        item = new Item
        {
            ItemInfo = items[ItemId],
            Count = Amount
        };
        var position = Vector2Int.RoundToInt((Vector2)transform.position);
        //ServiceManager.Get<TriggerManager>().AddTrigger(position, this);
    }

    public void OnEnter(TriggerParams triggerParams) { }

    public void OnExit(TriggerParams triggerParams) { }

    public void OnStay(TriggerParams triggerParams) { }

    public void OnUse(TriggerParams triggerParams)
    {
        if (gaveItem)
            return;
        LogManager.LogDebug($"Giving Item [{item.ItemInfo.Id}] in {name}.");
        var message = $"You got {item.ItemInfo.GetName()}";
        if (item.ItemInfo.Type == ItemType.Key)
            ServiceManager.Get<World>().AddKeyItem(item.ItemInfo);
        else
            ServiceManager.Get<World>().AddItem(item);
        gaveItem = true;
        ServiceManager.Get<GameLogic>().Stack.PushTextbox(message);
    }
}
