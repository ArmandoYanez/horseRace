using UnityEngine;

// -------------------- STARTING POINTS (afecta al caballo)
[CreateAssetMenu(menuName = "Carnival/EventEffects/StartingPointsBonus")]
public class StartingPointsBonusEffect : EventEffectSO
{
    public int roundsAhead = 1;
    public int startingBonus = 5;

    public override void Apply(EventContext context)
    {
        //if (context.playerHorse == null) return;

        context.gameManager.ScheduleStartingPointsBonus(
            context.currentRound + roundsAhead,
            startingBonus
        );
    }
}
