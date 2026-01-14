using UnityEngine;

[CreateAssetMenu(menuName="Carnival/Items/Soda")]
public class SodaEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.ScheduleStartingPointsBonus(gm.currentRound + 1, 3);
    }
}

