using MoreMountains.Feedbacks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Inventory")]
    public InventorySlot[] inventorySlots; 
    public int maxItems = 5;
    public InventoryUI inventoryUI;
    public MMF_Player feedback;
    public GameManager gameManager;
    public void TryBuyItem(ShopItemSO item)
    {
        // 1. Verificar dinero
        if (!EconomyManager.Instance.CanAfford(item.price))
        {
            Debug.Log("❌ Not enough money");
            return;
        }

        // 2. Pagar
        if (!EconomyManager.Instance.SpendMoney(item.price))
        {
            Debug.Log("❌ Payment failed");
            return;
        }

        // 3. ¿Instantáneo o consumible?
        if (item.itemType == ShopItemType.Instant)
        {
            Debug.Log($"⚡ Instant item applied: {item.title}");

            item.effect.Apply(gameManager);

            return; 
        }

        // 4. Consumible → inventario
        AddToInventory(item);
    }

    void AddToInventory(ShopItemSO item)
    {
        if (CurrentItemCount() >= maxItems)
        {
            Debug.Log("Inventory full (5 items max)");
            return;
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.IsFree)
            {
                slot.active();
                slot.SetItem(item);
                feedback.PlayFeedbacks();
                //inventoryUI.Refresh(); 
                return;
            }
        }
    }

    
    int CurrentItemCount()
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (!slot.IsFree)
                count++;
        }
        return count;
    }
}