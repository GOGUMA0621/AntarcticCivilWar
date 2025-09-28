using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitDetectTarget : MonoBehaviour //유닛 적 탐지
{
    public IDamageAble targetToAttack; //공격할 타깃의 위치값
    public List<IDamageAble> targets; // 타겟 리스트
    private Unit unit;
    
    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    internal void SortClosestTarget() //타겟 리스트를 가까운 순으로 정렬하여 공격할 상대값에 값 부여
    {                                   //특정 인터페이스를 후순위로 정렬
        targets = targets
            .OrderBy(t => t is MonoBehaviour mb && mb.TryGetComponent(out IStructure _) ? 1 : 0) // 특정 레이어를 가진 객체를 후순위로 배치
            .ThenBy(t => Vector2.Distance(transform.position, t.GetTransform().position)) // 같은 그룹 내에서는 거리순 정렬
            .ToList();

        if (targets.Any())
        {
            var firstTarget = targets.First();
            targetToAttack = firstTarget;
        }
    }

    public void AddTarget(IDamageAble target) //타깃 리스트에 추가
    {
        if (!targets.Contains(target) && !target.IsDestroyed())
        {
            target.OnDestroyed += RemoveTarget;  //타깃 리스트에 들어가면서 파괴확인 이벤트에 등록
            targets.Add(target);
            SortClosestTarget();
        }
    }
    public void AddTargets(List<IDamageAble> newTargets) //타깃 리스트에 여러개 추가
    {
        foreach (var target in newTargets)
        {
            AddTarget(target);
        }
    }

    public void RemoveTarget(IDamageAble target) // 타깃 리스트에서 제거
    {
        if (targets.Contains(target))
        {

                //Debug.Log($"{this.gameObject}의 타겟 제거 {target.gameObject}");
                target.OnDestroyed -= RemoveTarget; //타깃 리스트에 존재 하지 않으므로 이벤트에서 제거
                targets.Remove(target);


            if (target == targetToAttack)
            {
                targetToAttack = null;
            }
            SortClosestTarget();
        }
    }

    public void ClearTarget() //타깃 리스트 초기화
    {
        //Debug.Log("타겟 클리어");
        targets.Clear();
        targetToAttack = null;
    }
}
