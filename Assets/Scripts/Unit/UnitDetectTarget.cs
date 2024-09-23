using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitDetectTarget : MonoBehaviour
{
    [HideInInspector] public Transform targetToAttack;
    public List<UnitController> targets;

    private void Update()
    {
        if (targetToAttack == null && targets.Any())
        {
            AttackClosestTarget();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(this.tag);
        if(other.TryGetComponent<UnitController>(out UnitController target))
        {
            if (target.CompareTag("Enemy") && this.transform.parent.CompareTag("Unit") && !targets.Contains(target))
            {
            
                targets.Add(target);
                if (targetToAttack == null)
                {
                    AttackClosestTarget();
                }
            }
            if (target.CompareTag("Unit") && this.transform.parent.CompareTag("Enemy") && !targets.Contains(target))
            {
                targets.Add(target);
                if (targetToAttack == null)
                {
                    AttackClosestTarget();
                }
            }
        }
        //if (other.CompareTag("Enemy") && this.CompareTag("Unit"))
        //{
        //    UnitController target = other.GetComponent<UnitController>();
        //    targets.Add(target);
        //    if (targetToAttack == null)
        //    {
        //        AttackClosestTarget();
        //    }
        //}
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        
    }

    void AttackClosestTarget()
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

    public void RemoveTarget(UnitController target)
    {
        targets.Remove(target);
        targetToAttack = null;
    }
}
