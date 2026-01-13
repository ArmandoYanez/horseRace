using UnityEngine;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    public Transform prefabAnchor;      // Donde se instancia el prefab
    public TextMeshPro titleText;
    public TextMeshPro priceText;

    private ShopItemSO currentItem;

    public void Setup(ShopItemSO item)
    {
        currentItem = item;

        // Limpia visual previo
        foreach (Transform c in prefabAnchor)
            Destroy(c.gameObject);

        if (item.prefab != null)
        {
            Instantiate(item.prefab, prefabAnchor);
        }

        titleText.text = item.title;
        priceText.text = item.price.ToString();
    }

    public ShopItemSO GetItem()
    {
        return currentItem;
    }
}