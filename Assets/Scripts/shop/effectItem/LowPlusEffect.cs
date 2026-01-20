using UnityEngine;

[CreateAssetMenu(menuName = "Carnival/Items/LowPlus")]
public class LowPlusEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.lowShotModifier += 1;
        Debug.Log("Low shots +1 forever");
    }
}
