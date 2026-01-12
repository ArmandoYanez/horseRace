using UnityEngine;

// -------------------- CONDICIÓN DE PERDER (no depende del caballo)
[CreateAssetMenu(menuName = "Carnival/EventEffects/RequireLossForBonus")]
public class RequireLossForBonusEffect : EventEffectSO
{
    public int roundsAhead = 1;
    public int bonusAmount = 1;

    public override void Apply(EventContext context)
    {
        context.gameManager.SchedulePermanentAllShotsOnLoss(
            context.currentRound + 1
        );
    }
}