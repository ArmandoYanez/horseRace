using UnityEngine;

[CreateAssetMenu(menuName="Carnival/Items/Swap")]
public class SwapEffect : ItemEffectSO
{
    public int midAmount = 2;

    public override void Apply(GameManager gm)
    {
        gm.swapMidActive = true;
        gm.midShotModifier += midAmount;
    }
}