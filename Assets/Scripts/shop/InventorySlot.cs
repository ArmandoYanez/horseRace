using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public Button useButton;
    public MMF_Player feedback;
    private ShopItemSO item;

    void Awake()
    {
        Clear();
    }

    public bool IsFree => item == null;

    public void active()
    {
        feedback.PlayFeedbacks();
    }
    public void SetItem(ShopItemSO newItem)
    {
        item = newItem;
        iconImage.sprite = item.icon;
        iconImage.enabled = true;

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(UseItem);
    }

    void UseItem()
    {
        Debug.Log($"Using item: {item.title}");

        // más adelante: aplicar efecto
        Clear();
    }

    void Clear()
    {
        item = null;
        iconImage.enabled = false;
        useButton.onClick.RemoveAllListeners();
    }
}