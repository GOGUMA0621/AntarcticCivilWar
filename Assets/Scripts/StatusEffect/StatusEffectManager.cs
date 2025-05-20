using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffectInstance> activeStatusEffects = new List<StatusEffectInstance>();
    private Dictionary<StatusEffectInstance, StatusEffectSO> instanceToSO = new Dictionary<StatusEffectInstance, StatusEffectSO>();

    public event Action<StatusEffectSO> OnStatusEffectApplied;
    public event Action<StatusEffectSO, float> OnStatusEffectUpdated;
    public event Action<StatusEffectSO> OnStatusEffectRemoved;

    private IDamageAble target;

    private void Awake()
    {
        target = transform.root.GetComponent<IDamageAble>();

    }

    private void Update()
    {
        foreach(var statusEffect in activeStatusEffects)
        {
            statusEffect.UpdateCall(Time.deltaTime);
            if(statusEffect.ShouldTick())
            {
                statusEffect.Tick();
            }

            if (instanceToSO.TryGetValue(statusEffect, out var so))
            {
                float normalizedDuration = Mathf.Clamp01(statusEffect.remainingDuration / statusEffect.source.activeDuration);
                OnStatusEffectUpdated?.Invoke(so, normalizedDuration);
            }

            if (statusEffect.IsExpired())
            {
                RemoveStatusEffect(statusEffect);
            }
        }
    }

    public void ApplyStatusEffect(StatusEffectSO so)
    {
        var instance = activeStatusEffects.Find(x => x.source == so);

        if (instance != null)
        {
            RemoveStatusEffect(instance);
        }

        instance = new StatusEffectInstance(so);
        activeStatusEffects.Add(instance);

        OnStatusEffectApplied?.Invoke(so);
    }

    private void RemoveStatusEffect(StatusEffectInstance instance)
    {
        if(instanceToSO.TryGetValue(instance, out var so))
        {
            OnStatusEffectRemoved?.Invoke(so);
        }

        activeStatusEffects.Remove(instance);
        instanceToSO.Remove(instance);
    }
}
