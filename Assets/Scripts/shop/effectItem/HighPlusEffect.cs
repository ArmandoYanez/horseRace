using UnityEngine;

[CreateAssetMenu(menuName = "Carnival/Items/HighPlus")]
public class HighPlusEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.highShotModifier += 1;
        Debug.Log("High shots +1 forever");
    }
}

