using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StunStatusEffectSO",menuName = "Scriptable Object/Status Effect/StunStatusEffect")]
public class StunStatusEffectSO : StatusEffectSO
{
    float currentSpeed = 0f;

    public override void ApplyEffect(GameObject target)
    {
        base.ApplyEffect(target);

        if (isEffectActive)
        {
            currentSpeed = unit.controller.unitSpeed;
            unit.controller.isStunned = true;
            unit.animator.ResetTrigger("attack");
            unit.controller.unitSpeed = 0f;
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        base.RemoveEffect(target);
        if (unit != null)
        {
            unit.controller.isStunned = false;
            unit.controller.unitSpeed = currentSpeed;
        }
    }
}
