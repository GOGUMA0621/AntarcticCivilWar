using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitDetectTarget : MonoBehaviour
{
    internal CircleCollider2D detectRadiusCollider;
    private Unit _unit;
    [HideInInspector] public Transform targetToAttack;
    public List<Unit> targets;

    private void Start()
    {
        detectRadiusCollider = this.GetComponent<CircleCollider2D>();
        _unit = this.transform.parent.GetComponent<Unit>();
    }

    private void Update()
    {
        if (targetToAttack == null && targets.Any())
        {
            AttackClosestTarget();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(this.transform.parent.tag);
        if(other.TryGetComponent<Unit>(out Unit target))
        {
            if (target.tag != this.transform.parent.tag)
            {
                if (target.CompareTag("Enemy"))
                {
                    _unit.playerUnitManager.AddEnemyList(target);
                    //AddTarget(target);
                }
                if (target.CompareTag("Unit"))
                {
                    AddTarget(target);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
            
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
            targets.Remove(target);
            //Debug.Log("일반 타겟"+target.ToString());
            if (target.transform == targetToAttack)
            {
                Debug.Log(target.ToString());
                targetToAttack = null;
            }
        }
    }

    public void StopCollider()
    {
        detectRadiusCollider.enabled = false;
    }
    public void StartCollider()
    {
        detectRadiusCollider.enabled = true;
    }

    public void ClearTarget()
    {
        targets.Clear();
        targetToAttack = null;
    }
}
