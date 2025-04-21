
using Pathfinding;
using SciptableObjects;
using UnityEditor;

using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour //유닛에 대한 부모 파일
{

    public GameObject originPrefab;
    public UnitData data;
    public UnitController controller { get; private set; }
    public UnitDistinction distinction { get; private set; }
    public UnitDetectTarget detectTarget { get; private set; }
    public UnitAttackController attackController { get; private set; }
    public AIPath aiPath { get; private set; }
    public Seeker seeker { get; private set; }
    public AIDestinationSetter settler { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public CapsuleCollider2D capsuleCollider { get; private set; }
    public SpriteRenderer spriteRenderer;
    [HideInInspector] public PlayerController playerController;
    public PlayerUnitManager playerUnitManager;
    public Animator animator { get; private set; }

    protected virtual void Awake()
    {
        controller = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerController = GameObject.FindAnyObjectByType<PlayerController>();
        distinction = GetComponentInChildren<UnitDistinction>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        attackController = GetComponent<UnitAttackController>();
        aiPath = GetComponent<AIPath>();
        settler = GetComponent<AIDestinationSetter>();
    }
    public Unit GetUnit() { return this; }
    public SciptableObjects.UnitData GetData() { return data; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(originPrefab == null)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefab != null)
            {
                originPrefab = prefab;
                EditorUtility.SetDirty(this); // 변경사항 저장
            }
        }
    }
#endif
}
