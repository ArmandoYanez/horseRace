using UnityEngine;
public enum ShopItemType
{
    Instant,     // se ejecuta al comprar
    Consumable   // se guarda para usar después
}

[CreateAssetMenu(menuName = "Carnival/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("Visual")]
    public GameObject prefab;      // Prefab visual del item (icono / modelo)
    public Sprite icon;
    
    [Header("Info")]
    public string title;
    [TextArea]
    public string description;

    [Header("Cost")]
    public int price;
    
    [Header("Type")]
    public ShopItemType itemType;
    public ItemEffectSO effect;
}

