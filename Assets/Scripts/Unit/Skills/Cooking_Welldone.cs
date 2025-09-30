using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class Cooking_Welldone : MonoBehaviour,ISkill,IActiveSkill
{
    [Range(0f, 360f)]

    [HideInInspector] public List<Transform> targets;
    private Unit unit;
    public int skillDamage = 25;
    public float angleRange = 30f;
    public float radius = 3f;

    public bool IsDurationSkill => throw new System.NotImplementedException();

    public bool IsStandingSkill => throw new System.NotImplementedException();

    public float Duration => throw new System.NotImplementedException();

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    public void ActivateSkill()
    {
        foreach (Transform t in targets)
        {
            Vector2 interV = t.position - transform.position;

            if (interV.magnitude <= radius)
            {
                float dot = Vector2.Dot(interV.normalized, unit.detectTarget.targetToAttack.GetTransform().position);
                float theta = Mathf.Acos(dot);
                float degree = Mathf.Rad2Deg * theta;

                if (degree <= angleRange / 2f)
                {
                    t.GetComponent<Unit>().controller.ReceiveDamage(new DamageData(skillDamage, StatusEffectType.Burn , 5));
                }
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        // DrawSolidArc(������, ��ֺ���(��������), �׷��� ���� ����, ����, ������)
        Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, angleRange / 2, radius);
        Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -angleRange / 2, radius);
    }

    public void ActivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public void DeactivateSkill(UnitController unit)
    {
        throw new System.NotImplementedException();
    }
#endif
}
