using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cooking_CutMark : MonoBehaviour, ISkill, IPasseiveSkillAttack
{
    public GameObject cutMarkPrefab;
    private Animator animator;
    public float skillDamage;
    public float skillRadius;
    private Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
        animator = GetComponent<Animator>();
    }

    public void CutMarkEffect()
    {
        if (unit.unitDetectTarget.targetToAttack != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.unitDetectTarget.targetToAttack.position, skillRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out IDamageAble i) && !i.IsDestroyed() && !collider.CompareTag(this.tag))
                {
                    GameObject effect = Instantiate(cutMarkPrefab, unit.transform.position, Quaternion.identity,unit.transform);
                }
            }
        }
    }

    public void DoPassiveSkill()
    {
        animator.Play("PassiveSkill");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.unitDetectTarget.targetToAttack.position,skillRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Unit>(out Unit unit))
            {
                unit.unitController.ReceiveDamage(new DamageData(skillDamage, StatusEffectType.None, 0));
            }
        }

    }

    public bool PassiveCondition()
    {
        if (unit.unitDetectTarget.targetToAttack != null)
        {
            if (unit.unitDetectTarget.targetToAttack.TryGetComponent<Unit>(out Unit target))
            {
                return target.unitController.currentHP <= target.unitController.maxHP / 10;
            }
        }
        return false;
    }
}
