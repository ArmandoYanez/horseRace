using UnityEngine;

// -------------------- LOW BONUS FUTURO (afecta al caballo)
[CreateAssetMenu(menuName = "Carnival/EventEffects/LowBonusFuture")]
public class LowBonusFutureEffect : EventEffectSO
{
    [Header("When")]
    public int roundsAhead = 1;

    [Header("Effect")]
    public int bonusAmount = 1;

    public override void Apply(EventContext context)
    {
        //if (context.playerHorse == null) return;

        int targetRound = context.currentRound + roundsAhead;

        context.gameManager.ScheduleLowRiskBonus(
            targetRound,
            bonusAmount
        );
    }    
}