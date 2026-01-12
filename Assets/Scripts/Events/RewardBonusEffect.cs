using UnityEngine;

// -------------------- REWARD BONUS (no depende del caballo)
[CreateAssetMenu(menuName = "Carnival/EventEffects/RewardBonus")]
public class RewardBonusEffect : EventEffectSO
{
    public int roundsAhead = 1;
    public int extraReward = 5;

    public override void Apply(EventContext context)
    {
        context.gameManager.ScheduleRewardBonus(
            context.currentRound + roundsAhead,
            extraReward
        );
    }
}