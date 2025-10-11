using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TamerWalrus : MonoBehaviour, IActiveSkill

{
    public bool IsDurationSkill => false;

    public bool IsStandingSkill => true;

    public float Duration => 0f;
    
    [SerializeField] private GameObject _walrusPrefab;
    private UnitController unit;

    public void ActivateSkill(UnitController unit)
    {
        this.unit = unit;
        unit.isSkillActive = true;
    }

    public void DeactivateSkill(UnitController unit)
    {
        unit.isSkillActive = false;
    }
    public void SummmonWalrus()
    {
        Instantiate(_walrusPrefab, this.transform.position, Quaternion.identity);
    }
}
