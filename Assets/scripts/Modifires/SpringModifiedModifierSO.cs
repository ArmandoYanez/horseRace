using UnityEngine;

[CreateAssetMenu(menuName = "Fair/Modifiers/Spring Modified")]
public class SpringModifiedModifierSO : ModifierSO, IShotModifier
{
    public override void Apply(HorseRuntime horse)
    {
        horse.activeModifiers.Add(this);
    }

    public override void Remove(HorseRuntime horse)
    {
        horse.activeModifiers.Remove(this);
    }

    public int ModifyShotPoints(int currentPoints, ShotType shotType)
    {
        // Se aplica a cualquier tiro
        return Random.value < 0.5f ? currentPoints + 2 : currentPoints - 1;
    }
}