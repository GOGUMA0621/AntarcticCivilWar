using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodyClaw : OnHitItem
{
    
    
    public override void ApplyEffect(UnitController unit)
    {
        unit.RegisterOnHitEffect(this);
    }

    public override void OnHit(UnitController unit, IDamageAble target)
    {
        float amount = effectBaseValue[0] + (effectStackValue[0] * currentStack);
        unit.Heal(amount);
    }

    public override void UpdateEffect(UnitController unit)
    {
        return;
    }
}
