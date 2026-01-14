using UnityEngine;

[CreateAssetMenu(menuName="Carnival/Items/RatTrap")]
public class RatTrapEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.BlockLeaderNextTurn = true;
    }
}