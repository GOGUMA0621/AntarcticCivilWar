using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class StatusEffectManager : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<StatusEffectType, StatusEffectSO> statusEffectToApplyListDict = new();

    private SerializedDictionary<StatusEffectType, StatusEffectSO> enabledEffects = new();

    private Dictionary<StatusEffectType, StatusEffectSO> statusEffectCacheDict = new Dictionary<StatusEffectType, StatusEffectSO>();

    [SerializeField, Tooltip("StatusEffectSO에서 UpdateCall이 어떤 간격에서 동작하는가")] private float interval = .1f;
    private float currentInterval = 0f;
    private float lastInterval = 0f;

    public UnityAction<StatusEffectSO, float> ActiveStatus;
    public UnityAction<StatusEffectSO> DeactiveStatusEffect;
    public UnityAction<StatusEffectSO, float, float> UpdateStatusEffect;

    private void Start()
    {
        
    }

    private void Update()
    {
        currentInterval += Time.deltaTime;
        if (currentInterval > lastInterval + interval)
        {
            UpdateEffects(gameObject);
            lastInterval = currentInterval;
        }
    }

    public void OnStatusTriggerBuildup(StatusEffectType effectType, float buildAmount)
    {
        if (!enabledEffects.ContainsKey(effectType))
        {
            var effectToAdd = CreateEffctObject(effectType, statusEffectToApplyListDict[effectType]);

            enabledEffects[effectType] = effectToAdd;

            ActiveStatus?.Invoke(effectToAdd, effectToAdd.GetCurrentDurationNormalized()); 
        }
        if (!enabledEffects[effectType].isEffectActive)
        {
            enabledEffects[effectType].AddBuildup(buildAmount, gameObject);
            UpdateStatusEffect?.Invoke(enabledEffects[effectType], enabledEffects[effectType].GetCurrentThresholdNormalized(),
                enabledEffects[effectType].GetCurrentDurationNormalized());
        }
        else
        {
            int tickDamageAmount = (int)Mathf.Ceil(buildAmount / 4);
        }

    }

    private StatusEffectSO CreateEffctObject(StatusEffectType effectType, StatusEffectSO statusEffect)
    {
        if (!statusEffectCacheDict.ContainsKey(effectType))
        {
            statusEffectCacheDict[effectType] = Instantiate(statusEffect);
        }

        return statusEffectCacheDict[effectType];
    }

    public void UpdateEffects(GameObject target)
    {
        foreach (var effect in enabledEffects.ToList())
        {
            effect.Value.UpdateCall(target, interval);

            UpdateStatusEffect?.Invoke(effect.Value, effect.Value.GetCurrentThresholdNormalized(), effect.Value.GetCurrentDurationNormalized());

            if (effect.Value.CanStatusVisualBeRemoved())
            {
                RemoveEffect(effect.Key);
            }
        }
    }

    private void RemoveEffect(StatusEffectType effectType)
    {
        if (enabledEffects.ContainsKey(effectType))
        {
            DeactiveStatusEffect?.Invoke(enabledEffects[effectType]);

            enabledEffects[effectType].RemoveEffect(gameObject);
        }
    }
}
