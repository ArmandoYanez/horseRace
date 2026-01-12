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

