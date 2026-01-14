using UnityEngine;

[CreateAssetMenu(menuName="Carnival/Items/Dice")]
public class DiceEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.playerExtraRolls += 1;
        Debug.Log("Extra roll granted!");
    }
}

