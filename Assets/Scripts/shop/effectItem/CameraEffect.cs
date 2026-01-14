using UnityEngine;

[CreateAssetMenu(menuName = "Carnival/Items/Camera")]
public class CameraEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.leaderPenaltyNextTurn = 2;
    }
}


