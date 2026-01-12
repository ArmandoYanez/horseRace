using UnityEngine;

// -------------------- ENEMY DEBUFF (no depende del caballo del jugador)
[CreateAssetMenu(menuName = "Carnival/EventEffects/EnemyDebuffFuture")]
public class EnemyDebuffFutureEffect : EventEffectSO
{
    public int roundsAhead = 1;
    public int failPenalty = 1;

    public override void Apply(EventContext context)
    {
        context.gameManager.ScheduleEnemyDebuff(
            context.currentRound + roundsAhead,
            failPenalty
        );
    }
}
