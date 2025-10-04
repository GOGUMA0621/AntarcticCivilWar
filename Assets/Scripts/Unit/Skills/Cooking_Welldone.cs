using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class Cooking_Welldone : MonoBehaviour,ISkill,IActiveSkill
{
    [Range(0f, 360f)]

    [HideInInspector] public List<Transform> targets;
    private UnitController unit;
    public int skillDamage = 25;
    public float angleRange = 30f;
    public float radius = 3f;

    public bool IsDurationSkill => false;

    public bool IsStandingSkill => true;

    public float Duration => 0f;

    private List<DamageData> damageDatas = new List<DamageData>();

    private void Start()
    {
    }

    public void WelldoneSkill()
    {
        DamageData damageData = damageDatas[unit.unitLevel - 1];

        foreach (Transform t in targets)
        {
            Vector2 interV = t.position - transform.position;

            if (interV.magnitude <= radius)
            {
                float dot = Vector2.Dot(interV.normalized, unit.unit.detectTarget.targetToAttack.GetTransform().position);
                float theta = Mathf.Acos(dot);
                float degree = Mathf.Rad2Deg * theta;

                if (degree <= angleRange / 2f)
                {
                    t.GetComponent<Unit>().controller.ReceiveDamage(damageData);
                    Debug.Log($"{unit.name}의 {this.GetType().Name} 스킬로 {t.name}에게 {damageData.damage} 피해를 입혔습니다.");
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
        this.unit = unit;
    }

    public void DeactivateSkill(UnitController unit)
    {
       unit.isSkillActive = false;
    }
#endif
}
