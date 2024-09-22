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
        if (other.CompareTag("Enemy") && this.transform.parent.CompareTag("Unit"))
        {
            UnitController target = other.GetComponent<UnitController>();
            targets.Add(target);
            if (targetToAttack == null)
            {
                AttackClosestTarget();
            }
        }
        if (other.CompareTag("Unit") && this.transform.parent.CompareTag("Enemy"))
        {
            UnitController target = other.GetComponent<UnitController>();
            targets.Add(target);
            if (targetToAttack == null)
            {
                AttackClosestTarget();
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
        if (!targets.Any())
        {
            targetToAttack = null;
        }
    }
}
