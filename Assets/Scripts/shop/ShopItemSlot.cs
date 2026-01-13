using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    public Transform prefabAnchor;
    public TextMeshPro titleText;
    public TextMeshPro DescriptionText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ShopItemSO item;
    private ShopManager shopManager;

    public void Setup(ShopItemSO newItem, ShopManager manager)
    {
        item = newItem;
        shopManager = manager;

        foreach (Transform c in prefabAnchor)
            Destroy(c.gameObject);

        if (item.prefab != null)
            Instantiate(item.prefab, prefabAnchor);

        titleText.text = item.title;
        DescriptionText.text = item.description;
        priceText.text = "buy " + item.price.ToString()+"$";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(TryBuy);
    }

    void TryBuy()
    {
        shopManager.TryBuyItem(item);
    }
}