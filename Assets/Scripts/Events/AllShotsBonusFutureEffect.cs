using UnityEngine;

// -------------------- ALL SHOTS BONUS (afecta al caballo)
[CreateAssetMenu(menuName = "Carnival/EventEffects/AllShotsBonusFuture")]
public class AllShotsBonusFutureEffect : EventEffectSO
{
    public int roundsAhead = 1;
    public int bonusAmount = 1;

    public override void Apply(EventContext context)
    {
        //if (context.playerHorse == null) return;

        context.gameManager.ScheduleAllShotsBonus(
            context.currentRound + roundsAhead,
            bonusAmount
        );
    }
}
