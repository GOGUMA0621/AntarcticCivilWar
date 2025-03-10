using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedStatusEffectSO", menuName ="Scriptable Object/Status Effect/BleedStatusEffect")]
public class BleedStatusEffectSO : StatusEffectSO
{
    public float tickDamage;

    public override void UpdateEffect(GameObject target)
    {
        base.UpdateEffect(target);
        if (isEffectActive)
        {
            unit.unitController.ReceiveDamage(new DamageData(tickDamage, statusEffectType, 0));
        }
    }
}
