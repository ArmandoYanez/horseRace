using UnityEngine;

[CreateAssetMenu(menuName = "Carnival/Items/PepperSpray")]
public class PepperSprayEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.BlockAllEnemiesNextTurn();
    }
}