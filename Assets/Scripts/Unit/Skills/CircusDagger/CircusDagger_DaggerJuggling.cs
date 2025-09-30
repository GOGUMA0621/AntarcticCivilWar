using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircusDagger_DaggerJuggling : MonoBehaviour, IActiveSkill
{
    [SerializeField] private float skillDuration = 5f;

    public bool IsDurationSkill => throw new System.NotImplementedException();

    public bool IsStandingSkill => throw new System.NotImplementedException();

    public float Duration => throw new System.NotImplementedException();

    public void ActivateSkill()
    {
        StartCoroutine(DaggerJugglingRoutine());
    }

    public void ActivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public void DeactivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator DaggerJugglingRoutine()
    {
        var unit = GetComponent<UnitController>();
        if (unit == null) yield break;
        unit.GoSkill(isStanding: false, duration: skillDuration);

        yield return new WaitForSeconds(skillDuration);
        
        unit.GoIdle();
    }
}
