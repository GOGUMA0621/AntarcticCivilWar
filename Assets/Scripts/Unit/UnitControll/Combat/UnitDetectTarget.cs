using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnitController;

public class UnitDetectTarget : Unit //유닛 적 탐지
{
    public Transform targetToAttack; //공격할 타깃의 위치값
    public List<GameObject> targets; // 타겟 리스트
    private Unit _unit;

    protected override void Start()
    {
        base.Start();
        _unit = GetComponent<Unit>();
        data = _unit.data;
    }

    private void Update()
    {
        
        if (targetToAttack == null && targets.Any())
        {
            AttackClosestTarget();
        }
    }
    private void FixedUpdate()
    {
        Detect();
    }

    internal void AttackClosestTarget() //타겟 리스트를 가까운 순으로 정렬하여 공격할 상대값에 값 부여
    {
        targets.Sort((a, b) =>
        {
            float distanceA = Vector2.Distance(this.transform.position, a.transform.position);
            float distanceB = Vector2.Distance(this.transform.position, b.transform.position);

            return distanceA.CompareTo(distanceB);
        });

        if (targets.Any())
        {
            targetToAttack = targets.First().transform;
        } 
    }

    public void AddTarget(GameObject target) //타깃 리스트에 추가
    {
        //Debug.Log("타켓 발견");
        if (!targets.Contains(target) && target.tag != this.tag)
        {
            targets.Add(target);
            if (targetToAttack == null)
            {
                AttackClosestTarget();
            }
        }
    }

    public void RemoveTarget(GameObject target) // 타깃 리스트에서 제거
    {
        if (targets.Contains(target))
        {
            targets.RemoveAt(targets.IndexOf(target));
            if (target.transform == targetToAttack)
            {
                targetToAttack = null;
            }
        }
    }

    void Detect() //타깃 감지 메소드
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, data.UnitSenseRadius);
        foreach(Collider2D targetCollider in collider)
        {
            if(targetCollider.gameObject.TryGetComponent<Unit>(out Unit target) && targetCollider.tag != this.gameObject.tag)
            {
                if (this.gameObject.tag != "Unit" && targetCollider.gameObject.tag == "Mercenary") return;
                else if (this.gameObject.tag == "Mercenary" && targetCollider.gameObject.tag != "Unit") return;
                else if (!target.unitController.isUnitDie) AddTarget(target.gameObject);
            }
            else if(targetCollider.gameObject.TryGetComponent<INeutrality>(out INeutrality neutrality) && this.gameObject.tag == "Unit")
            {
                if (neutrality is MonoBehaviour neautralityObject)
                {
                    AddTarget(neautralityObject.gameObject);
                }
            }
        }
    }

    public void ClearTarget() //타깃 리스트 초기화
    {
        //Debug.Log("타겟 클리어");
        targets.Clear();
        targetToAttack = null;
    }

    private void OnEnable()
    {
        UnitController.OnUnitDeath += RemoveTarget; //유닛 죽음 감지로 리스트 제거
    }

    private void OnDisable()
    {
        UnitController.OnUnitDeath -= RemoveTarget;
    }
}
