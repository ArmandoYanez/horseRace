using UnityEngine;

public static class AIShotChooser
{
    public static ShotType ChooseShot()
    {
        int roll = Random.Range(0, 3);

        return roll switch
        {
            0 => ShotType.Low,
            1 => ShotType.Medium,
            _ => ShotType.High
        };
    }
}