using UnityEngine;

public enum EventEffectType
{
    ModifyHorse,
    ModifyMachine,
    FutureBonus,
    RequireLoss,
    InfoOnly
}

public abstract class EventEffectSO : ScriptableObject
{
    public abstract void Apply(EventContext context);
}

public class EventContext
{
    public int currentRound;
    public HorseRuntime playerHorse;
    public GameManager gameManager;
}

#region Scheduled_Events

[System.Serializable]
public class ScheduledLowRiskBonus
{
    public int targetRound;
    public int bonusAmount;
}

[System.Serializable]
public class ScheduledAllShotsBonus
{
    public int targetRound;
    public int bonusAmount;
}

[System.Serializable]
public class ScheduledStartingPointsBonus
{
    public int targetRound;
    public int bonusPoints;
}

[System.Serializable]
public class ScheduledEnemyDebuff
{
    public int targetRound;
    public int failPenalty;
}

[System.Serializable]
public class ScheduledRewardBonus
{
    public int targetRound;
    public int extraReward;
}

[System.Serializable]
public class ScheduledLossCondition
{
    public int triggerRound;      // ronda en la que DEBES perder
    public int bonusTargetRound;  // ronda donde se aplicará el bonus
    public int bonusAmount;
}
#endregion



// Eventos personalizados que se pueden invocar en la partida
#region SO_Events

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

#endregion
