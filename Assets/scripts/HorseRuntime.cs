using System.Collections.Generic;

public class HorseRuntime
{
    public HorseSO baseData;

    // Estado dinámico
    public int currentPoints;
    public int shotsPerTurn;
    public int bonusPerRound;

    public bool allowLow;
    public bool allowMedium;
    public bool allowHigh;

    public List<ModifierSO> activeModifiers = new();

    public HorseRuntime(HorseSO so)
    {
        baseData = so;

        currentPoints = so.startingPoints;
        shotsPerTurn = so.shotsPerTurn;
        bonusPerRound = so.bonusPerRound;

        allowLow = so.allowLowShot;
        allowMedium = so.allowMediumShot;
        allowHigh = so.allowHighShot;
    }
}