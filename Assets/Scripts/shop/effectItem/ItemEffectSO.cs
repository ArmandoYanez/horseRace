using UnityEngine;

public abstract class ItemEffectSO : ScriptableObject
{
    public abstract void Apply(GameManager gameManager);
}
