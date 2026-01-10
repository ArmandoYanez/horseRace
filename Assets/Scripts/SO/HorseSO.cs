using UnityEngine;

public enum HorseType
{
    Red,
    Blue,
    Yellow,
    Green
}

public enum ShotType
{
    Low = 1,
    Medium = 2,
    High = 3
}

[CreateAssetMenu(
    fileName = "Horse_",
    menuName = "Fair/Horse"
)]
public class HorseSO : ScriptableObject
{
    [Header("Identity")]
    public string horseName;
    public HorseType type;

    [Header("Progress")]
    [Tooltip("Points the horse starts the race with")]
    public int startingPoints;

    [Tooltip("Points required to win the race")]
    public int pointsToWin = 15;

    [Header("Turn")]
    [Tooltip("How many shots per turn this horse can make")]
    public int shotsPerTurn = 1;

    [Header("Shot Restrictions")]
    public bool allowLowShot = true;
    public bool allowMediumShot = true;
    public bool allowHighShot = true;

    [Header("Base Bonuses")]
    [Tooltip("Guaranteed points added at the end of each round")]
    public int bonusPerRound;

    [Header("Cost")]
    [Tooltip("Base cost to use this horse")]
    public int baseCost;
}