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
            currentSpeed = unit.unitController.unitSpeed;
            unit.unitController.isStunned = true;
            unit.unitAnimator.ResetTrigger("attack");
            unit.unitController.unitSpeed = 0f;
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        base.RemoveEffect(target);
        if (unit != null)
        {
            unit.unitController.isStunned = false;
            unit.unitController.unitSpeed = currentSpeed;
        }
    }
}
