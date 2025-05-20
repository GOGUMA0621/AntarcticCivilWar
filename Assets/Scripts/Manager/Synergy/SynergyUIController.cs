using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyUIController : MonoBehaviour
{
    [SerializeField] private Transform SynergyListParent;
    [SerializeField] private GameObject SynergyUIPrefab;

    private void OnEnable()
    {
        SynergyManager.instance.OnSynergyUpdated += RenderSynergyUI;
        RenderSynergyUI();
    }
    
    private void OnDisable()
    {
        if(SynergyManager.instance == null) return;
        SynergyManager.instance.OnSynergyUpdated -= RenderSynergyUI;
    }

    public void RenderSynergyUI()
    {
        foreach (Transform child in SynergyListParent)
        {
            Destroy(child.gameObject);
        }

        var synergies = SynergyManager.instance.GetAllaySynergyData();

        foreach (var synergy in synergies)
        {
            Debug.Log($"Synergy: {synergy.name}, Count: {synergy.count}, Tier: {synergy.tier}");
        }

        foreach (var synergy in synergies)
        {
            var synergyUI = Instantiate(SynergyUIPrefab, SynergyListParent);
            var synergyUIController = synergyUI.GetComponent<SynergyUI>();
            if (synergyUIController != null)
            {
                synergyUIController.SetSynergyData(synergy);
            }
            else
            {
                Debug.LogWarning("SynergyUIPrefab does not have a SynergyUI component.");
            }
        }
    }
}
