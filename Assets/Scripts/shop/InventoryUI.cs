using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI inventoryText;
    public ShopManager shopManager;

    public void Refresh()
    {
        int current = 0;

        foreach (var slot in shopManager.inventorySlots)
        {
            if (!slot.IsFree)
                current++;
        }

        inventoryText.text = $"{current} / {shopManager.maxItems}" + " ITEMS";
    }
}