using UnityEngine;

public enum ModifierCategory
{
    Horse,
    Machine,
    Global
}

public abstract class ModifierSO : ScriptableObject
{
    [Header("Info")]
    public string modifierName;
    public int cost;
    public ModifierCategory category;

    [TextArea]
    public string description;

    public abstract void Apply(HorseRuntime horse);
    public abstract void Remove(HorseRuntime horse);
}