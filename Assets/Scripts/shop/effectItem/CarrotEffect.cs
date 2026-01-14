using UnityEngine;

[CreateAssetMenu(menuName="Carnival/Items/Carrot")]
public class CarrotEffect : ItemEffectSO
{
    public override void Apply(GameManager gm)
    {
        gm.guaranteedBonusNextRoll += 2;
    }
}
