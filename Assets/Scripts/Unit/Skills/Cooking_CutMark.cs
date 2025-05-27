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
        if (unit.detectTarget.targetToAttack != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.detectTarget.targetToAttack.position, skillRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out IDamageAble i) && !i.IsDestroyed() && !collider.CompareTag(this.tag) && i is MonoBehaviour unit)
                {
                    Instantiate(cutMarkPrefab, unit.transform.position, Quaternion.identity,unit.transform);
                }
            }
        }
    }

    public void DoPassiveSkill()
    {
        animator.Play("PassiveSkill");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.detectTarget.targetToAttack.position,skillRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<IDamageAble>(out IDamageAble i) && !i.IsDestroyed() && !collider.CompareTag(unit.tag))
            {
                 i.ReceiveDamage(new DamageData(skillDamage, StatusEffectType.None, 0));
                Debug.Log($"{unit.name}의 {this.GetType().Name} 스킬로 {i}에게 {skillDamage} 피해를 입혔습니다.");
            }
        }

    }

    public bool PassiveCondition()
    {
        if (unit.detectTarget.targetToAttack != null)
        {
            if (unit.detectTarget.targetToAttack.TryGetComponent<Unit>(out Unit target))
            {
                return target.controller.GetNormalizedHealth() < 0.1f; 
            }
        }
        return false;
    }
}
