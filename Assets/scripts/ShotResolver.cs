using UnityEngine;

public static class ShotResolver
{
    public static int ResolvePureLuck(ShotType shot)
    {
        int required = shot switch
        {
            ShotType.Low => 2,
            ShotType.Medium => 3,
            ShotType.High => 4,
            _ => 1
        };

        int roll = Random.Range(1, required + 1);

        return roll == 1 ? (int)shot : 0;
    }
}
