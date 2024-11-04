using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitDetectTarget : Unit
{
    private Unit _unit;
    public Transform targetToAttack;
    public List<Unit> targets;

    void Start()
    {
        _unit = transform.parent.GetComponent<Unit>();
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
        DeathTarget();
    }

    internal void AttackClosestTarget()
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

    public void AddTarget(Unit target)
    {
        //Debug.Log("타켓 발견");
        if (!targets.Contains(target) && target.tag != this.transform.parent.tag)
        {
            targets.Add(target);
            if (targetToAttack == null)
            {
                AttackClosestTarget();
            }
        }
    }

    public void RemoveTarget(Unit target)
    {
        if (targets.Contains(target))
        {
            targets.RemoveAt(targets.IndexOf(target));
            //Debug.Log("일반 타겟"+target.ToString());
            if (target.transform == targetToAttack)
            {
                Debug.Log(target.ToString());
                targetToAttack = null;
            }
        }
    }

    void Detect()
    {
        if (!_unit.controller.isUnitDie)
        {
            Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, _unit.data.UnitSenseRadius);
            foreach(Collider2D targetCollider in collider)
            {
                if(targetCollider.gameObject.TryGetComponent<Unit>(out Unit target))
                {
                    if(!target.controller.isUnitDie) AddTarget(target);
                }
            }
        }
    }

    void DeathTarget()
    {
        if (targets.Any())
        {
            foreach (Unit target in targets)
            {
                if (target.controller.isUnitDie)
                {
                    RemoveTarget(target);
                    break;
                }
            }
        }
    }

    public void ClearTarget()
    {
        Debug.Log("타겟 클리어");
        targets.Clear();
        targetToAttack = null;
    }
}
