using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowPassive_SteelBolt : MonoBehaviour, IPasseiveSkillAttack
{
    private UnitController unit;
    private int attackCount = 0;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    // 일반 공격 시 호출
    public void DoPassiveSkill()
    {
        attackCount++;
        if (attackCount % 4 == 0)
        {
            // 10% 추가 피해 적용
            float baseDamage = unit.UnitStats.attackDamage;
            float bonus = baseDamage * 0.1f;
            Debug.Log($"강철 쇠뇌 추가 피해: {bonus}");
        }
    }

    public bool PassiveCondition()
    {
        return true;
    }
}
