using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnitController;

public class UnitDetectTarget : MonoBehaviour //유닛 적 탐지
{
    public Transform targetToAttack; //공격할 타깃의 위치값
    public List<GameObject> targets; // 타겟 리스트
    private Unit unit;
    private CircleCollider2D detectCollider;

    private void Start()
    {
        unit = transform.parent.GetComponent<Unit>();
        detectCollider = GetComponent<CircleCollider2D>();

        detectCollider.radius = unit.data.UnitSenseRadius;
    }

    internal void SortClosetTarget() //타겟 리스트를 가까운 순으로 정렬하여 공격할 상대값에 값 부여
    {                                   //특정 인터페이스를 후순위로 정렬
        targets = targets
            .OrderBy(t => t.TryGetComponent(out IStructure _) ? 1 : 0) // 특정 레이어를 가진 객체를 후순위로 배치
            .ThenBy(t => Vector2.Distance(transform.position, t.transform.position)) // 같은 그룹 내에서는 거리순 정렬
            .ToList();

        if (targets.Any())
        {
            targetToAttack = targets.First().transform;
        }
    }

    public void AddTarget(GameObject target) //타깃 리스트에 추가
    {
        if (!targets.Contains(target) && target.TryGetComponent(out IDamageAble i) && !i.IsDestroyed())
        {
            i.OnDestroyed += RemoveTarget;  //타깃 리스트에 들어가면서 파괴확인 이벤트에 등록
            targets.Add(target);
            SortClosetTarget();
        }
    }

    public void RemoveTarget(GameObject target) // 타깃 리스트에서 제거
    {
        if (targets.Contains(target))
        {
            
            if (target.TryGetComponent(out IDamageAble i))
            {
                //Debug.Log($"{this.gameObject}의 타겟 제거 {target.gameObject}");
                i.OnDestroyed -= RemoveTarget; //타깃 리스트에 존재 하지 않으므로 이벤트에서 제거
                targets.Remove(target);
            }
           
            if (target.transform == targetToAttack)
            {
                targetToAttack = null;
            }
            SortClosetTarget();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IDamageAble i) && i is MonoBehaviour target && target.tag != this.transform.parent.tag)
        {
            if (!target.IsDestroyed())
            {
                if (this.transform.parent.tag != "Unit" && (target.tag == "Mercenary")          //용병은 플레이어의 유닛만을 때리도록 수정
                    || (this.transform.parent.tag == "Mercenary" && target.tag != "Unit")
                    ) return;

                AddTarget(target.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveTarget(collision.gameObject);
    }

    public void ClearTarget() //타깃 리스트 초기화
    {
        //Debug.Log("타겟 클리어");
        targets.Clear();
        targetToAttack = null;
    }
}
