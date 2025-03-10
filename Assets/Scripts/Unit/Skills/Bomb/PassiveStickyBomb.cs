using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveStickyBomb : MonoBehaviour, IPasseiveSkillAttack
{
    int attackCount;
    bool canPassive;

    [SerializeField] private GameObject StickyBomb;

    private Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
        UnitController.OnUnitAttackCount += AttackCount;
    }

    public void DoPassiveSkill()
    {
        unit.unitAttackController.SetProjectile(StickyBomb);
        unit.unitAttackController.Attack();
    }

    public bool PassiveCondition()
    {
        return canPassive;
    }

    private void AttackCount()
    {
        unit.unitAttackController.ResetProjectile();
        canPassive = false;
        attackCount++;
        if (attackCount >= 4)
        {
            canPassive = true;
            attackCount = 0;
        }
    }
}
