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
        unit.attackController.SetProjectile(StickyBomb);
        unit.attackController.Attack();
    }

    public bool PassiveCondition()
    {
        return canPassive;
    }

    private void AttackCount()
    {
        unit.attackController.ResetProjectile();
        canPassive = false;
        attackCount++;
        if (attackCount >= 4)
        {
            canPassive = true;
            attackCount = 0;
        }
    }
}
