using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnStatusEffectSO", menuName = "Scriptable Object/Status Effect/BurnStatusEffect")]
public class BurnStatusEffectSO : StatusEffectSO
{
    public int tickDamage;
    public int duration;

    public override void UpdateEffect(GameObject target)
    {
        activeDuration = duration;
        base.UpdateEffect(target);
        if (isEffectActive)
        {
            unit.controller.ReceiveDamage(new DamageData(tickDamage, statusEffectType, 0));
        }
    }
}
