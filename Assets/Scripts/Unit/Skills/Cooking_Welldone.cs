using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.U2D;

public class Cooking_Welldone : MonoBehaviour,ISkill,IActiveSkill
{
    [Range(0f, 360f)]

    [HideInInspector] public List<Transform> targets;
    private Unit unit;
    public int skillDamage = 25;
    public float angleRange = 30f;
    public float radius = 3f;

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    public void DoActiveSkill()
    {
        foreach(GameObject target in unit.unitDetectTarget.targets)
        {
            if (target.TryGetComponent<Unit>(out Unit targetUnit))
            {
                this.targets.Clear();
                this.targets.Add(targetUnit.transform);
            }
        }
        foreach (Transform t in targets)
        {
            Vector2 interV = t.position - transform.position;

            if (interV.magnitude <= radius)
            {
                float dot = Vector2.Dot(interV.normalized, unit.unitDetectTarget.targetToAttack.position);
                float theta = Mathf.Acos(dot);
                float degree = Mathf.Rad2Deg * theta;

                if (degree <= angleRange / 2f)
                {
                    t.GetComponent<Unit>().unitController.ReceiveDamage(new DamageData(skillDamage, StatusEffectType.None , 0));
                }
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        // DrawSolidArc(시작점, 노멀벡터(법선벡터), 그려줄 방향 벡터, 각도, 반지름)
        Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, angleRange / 2, radius);
        Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -angleRange / 2, radius);
    }
#endif
}
