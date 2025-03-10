using System.Collections;
using System.Collections.Generic;
using UnityEditor.Purchasing;
using UnityEngine;

public enum StatusEffectType
{
    None,
    Slow,
    Freeze,
    Overwhelming,
    Electrocuted,
    Stun,
    Bleed,
    Burn
    
}

public abstract class StatusEffectSO : ScriptableObject
{
    public StatusEffectType statusEffectType;

    public float activationThreshold;
    public float thresholdReductionMultiplier = 1f;
    public float thresholdReductionEverySecond = 1f;

    public float activeDuration;

    public GameObject visualEffectPrefab;

    private float currentThreshold;
    private float remainingDuration;
    private GameObject vfxPlaying;

    [HideInInspector] public bool isBuildUpOnlyShow;
    [HideInInspector] public bool isEffectActive;

    public float tickInterval = .25f;
    private float tickIntervalCD;

    protected Unit unit;

    public virtual void AddBuildup(float buildUpAmount, GameObject target)
    {
        isBuildUpOnlyShow = true;
        currentThreshold += buildUpAmount;

        if((currentThreshold >= activationThreshold))
        {
            ApplyEffect(target);
        }
    }

    public virtual void ApplyEffect(GameObject target)
    {
        isEffectActive = true;
        remainingDuration = activeDuration;

        SetTargetData(target);

        if (visualEffectPrefab != null)
        {
            vfxPlaying = Instantiate(visualEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
        }
    }

    private void SetTargetData(GameObject target)
    {
        unit = target.GetComponent<Unit>();
    }

    public void UpdateCall(GameObject target, float tickAmount)
    {
        if (isEffectActive)
        {
            isBuildUpOnlyShow = false;

            remainingDuration -= tickAmount;

            if (remainingDuration <= 0)
            {
                isEffectActive = false;
            }
        }
        else
        {
            currentThreshold -= (tickAmount * thresholdReductionEverySecond) * thresholdReductionMultiplier;

            if(currentThreshold <= 0)
            {
                isBuildUpOnlyShow=false;
            }
        }

        tickIntervalCD += tickAmount;
        if (tickIntervalCD >= tickInterval)
        {
            UpdateEffect(target);
            tickIntervalCD = 0;
        }
    }

    public virtual void UpdateEffect(GameObject target)
    {

    }

    public virtual void RemoveEffect(GameObject target)
    {
        isEffectActive = false;
        currentThreshold = 0;
        remainingDuration = 0;

        if (vfxPlaying != null)
        {
            Destroy(vfxPlaying);
        }
    }

    public virtual bool CanStatusVisualBeRemoved()
    {
        return !(isEffectActive || isBuildUpOnlyShow);
    }

    public float GetCurrentThresholdNormalized()
    {
        return currentThreshold / activationThreshold;
    }

    public float GetCurrentDurationNormalized()
    {
        return remainingDuration / activeDuration;
    }
}
