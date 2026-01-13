using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

public class ShopUI : MonoBehaviour
{
    [Header("Feedbacks")]
    public MMF_Player enterFeedback;
    public MMF_Player exitFeedback;

    [Header("Slots")]
    public ShopItemSlot[] itemSlots; 

    [Header("Item Pool")]
    public List<ShopItemSO> allItems; // Todos los ítems posibles

    private System.Action onClose;

    public void Show(System.Action closeCallback)
    {
        gameObject.SetActive(true);
        onClose = closeCallback;

        GenerateItems();

        enterFeedback.PlayFeedbacks();
    }

    void GenerateItems()
    {
        List<ShopItemSO> pool = new List<ShopItemSO>(allItems);

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (pool.Count == 0)
            {
                itemSlots[i].gameObject.SetActive(false);
                continue;
            }

            int index = Random.Range(0, pool.Count);
            ShopItemSO item = pool[index];
            pool.RemoveAt(index);

            itemSlots[i].gameObject.SetActive(true);
            itemSlots[i].Setup(item);
        }
    }

    public void Close()
    {
        exitFeedback.PlayFeedbacks();
        onClose?.Invoke();
        gameObject.SetActive(false);
    }
}