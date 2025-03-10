using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnStatusEffectSO", menuName = "Scriptable Object/Status Effect/BurnStatusEffect")]
public class BurnStatusEffectSO : StatusEffectSO
{
    public int tickDamage;

    public override void UpdateEffect(GameObject target)
    {
        base.UpdateEffect(target);
        if (isEffectActive)
        {
            unit.unitController.ReceiveDamage(new DamageData(tickDamage, statusEffectType, 0));
        }
    }
}
