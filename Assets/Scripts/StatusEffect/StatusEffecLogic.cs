using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffectLogic : MonoBehaviour
{
    protected StatusEffectInstance instance;
    
    public virtual void Initialize(StatusEffectInstance instance)
    {
        this.instance = instance;
    }

    public abstract void ApplyEffect();
    public abstract void RemoveEffect();
    public abstract void UpdateEffect();
}
