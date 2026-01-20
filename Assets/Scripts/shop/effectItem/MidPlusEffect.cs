using UnityEngine;
    
[CreateAssetMenu(menuName = "Carnival/Items/MidPlus")]
public class MidPlusEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.midShotModifier += 1;
        Debug.Log("Mid shots +1 forever");
    }
}
