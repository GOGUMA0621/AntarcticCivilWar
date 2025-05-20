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

    public void UpdateCall(float deltaTime)
    {
        if (!isActive) return;

        remainingDuration -= deltaTime;
        tickTimer += deltaTime;
    }

    public bool ShouldTick()
    {
        return tickTimer >= source.tickInterval;
    }

    public void Tick()
    {
        tickTimer = 0f;
        logic.UpdateEffect();
    }

    public bool IsExpired()
    {
        return remainingDuration <= 0f;
    }
}
