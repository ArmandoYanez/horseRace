using UnityEngine;

[CreateAssetMenu(menuName = "Carnival/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("Visual")]
    public GameObject prefab;      // Prefab visual del item (icono / modelo)
    
    [Header("Info")]
    public string title;
    [TextArea]
    public string description;

    [Header("Cost")]
    public int price;
}

