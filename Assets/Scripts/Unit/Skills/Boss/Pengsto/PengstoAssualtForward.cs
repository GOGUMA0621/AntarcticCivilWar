using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengstoAssualtForward :MonoBehaviour, IActiveSkill
{
    private Animator animator;

    public bool IsDurationSkill => throw new System.NotImplementedException();

    public bool IsStandingSkill => throw new System.NotImplementedException();

    public float Duration => throw new System.NotImplementedException();

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ActivateSkill()
    {
        animator.Play("ManaSkill");
    }

    public void ActivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public void DeactivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }
}
