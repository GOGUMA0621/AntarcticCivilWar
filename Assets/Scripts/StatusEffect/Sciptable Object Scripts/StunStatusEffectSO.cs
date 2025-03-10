using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StunStatusEffectSO",menuName = "Scriptable Object/Status Effect/StunStatusEffect")]
public class StunStatusEffectSO : StatusEffectSO
{

    public override void ApplyEffect(GameObject target)
    {
        base.ApplyEffect(target);

        if (isEffectActive)
        {
            unit.unitController.isStunned = true;
            unit.unitAnimator.ResetTrigger("attack");
            unit.unitAgent.speed = 0;
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        base.RemoveEffect(target);
        if (unit != null)
        {
            unit.unitController.isStunned = false;
            unit.unitAgent.speed = unit.unitController.unitSpeed;
        }
    }
}
