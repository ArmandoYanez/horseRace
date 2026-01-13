using MoreMountains.Feedbacks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Inventory")]
    public InventorySlot[] inventorySlots; 
    public int maxItems = 5;
    public InventoryUI inventoryUI;
    public MMF_Player feedback;
    public void TryBuyItem(ShopItemSO item)
    {
        // 1. Verificar dinero
        if (!EconomyManager.Instance.CanAfford(item.price))
        {
            Debug.Log("Not enough money");
            return;
        }

        // 2. Intentar pagar
        bool paid = EconomyManager.Instance.SpendMoney(item.price);
        if (!paid)
        {
            Debug.Log("Payment failed");
            return;
        }

        // 3. Decidir qué hacer con el objeto
        if (item.itemType == ShopItemType.Instant)
        {
            Debug.Log($"⚡ Instant item applied: {item.title}");
            // item.effect.Apply(); ← luego
        }
        else
        {
            AddToInventory(item);
        }
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