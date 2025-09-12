using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectInstance : MonoBehaviour
{
    public StatusEffectSO source;
    public StatusEffectLogic logic;
    public GameObject iconObject;
    public GameObject vfxObject;

    public float remainingDuration;
    public bool isActive;

    private float tickTimer;

    public StatusEffectInstance (StatusEffectSO source)
    {
        this.source = source;
        remainingDuration = source.activeDuration;
        isActive = true;
        tickTimer = 0f;
    }

    public void OnManagerUpdate(float deltaTime)
    {
        if (!isActive) return;

        tickTimer += deltaTime;
        if (tickTimer >= source.tickInterval)
        {
            tickTimer = 0f;
            logic.UpdateEffect();
        }

        remainingDuration -= deltaTime;
        if (remainingDuration <= 0f)
        {
            isActive = false;
            logic.RemoveEffect();
        }
    }
}
