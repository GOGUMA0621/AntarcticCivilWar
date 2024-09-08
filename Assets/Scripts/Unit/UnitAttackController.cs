using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitAttackController : MonoBehaviour
{
    [HideInInspector] public Transform targetToAttack;
     public List<GameObject> targets;

    private void Update()
    {
        if (targetToAttack == null && targets.Any())
        {
            targets.Sort((a, b) =>
            {
                float distanceA = Vector2.Distance(this.transform.position, a.transform.position);
                float distanceB = Vector2.Distance(this.transform.position, b.transform.position);

                return distanceA.CompareTo(distanceB);
            });
            targetToAttack = targets.First().transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && targetToAttack == null)
        {
            GameObject target = other.gameObject;
            targets.Add(target);
            targetToAttack = target.transform;
        }
        Debug.Log(other.tag);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        
    }
}
