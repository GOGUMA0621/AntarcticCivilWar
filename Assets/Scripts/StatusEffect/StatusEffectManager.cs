using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffectInstance> activeStatusEffects = new List<StatusEffectInstance>();
    private Dictionary<StatusEffectInstance, StatusEffectSO> instanceToSO = new Dictionary<StatusEffectInstance, StatusEffectSO>();

    public Action<float> OnUpdateAll;

    private IDamageAble target;

    private void Awake()
    {
        target = transform.root.GetComponent<IDamageAble>();

    }

    private void Update()
    {

    }

    public void Register(StatusEffectInstance instance)
    {
        OnUpdateAll += instance.OnManagerUpdate;
        activeStatusEffects.Add(instance);
    }

    public void Unregister(StatusEffectInstance instance)
    {
        OnUpdateAll -= instance.OnManagerUpdate;
        activeStatusEffects.Remove(instance);
    }

}
