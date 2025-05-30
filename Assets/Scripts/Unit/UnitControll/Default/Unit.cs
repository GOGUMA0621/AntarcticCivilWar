
using SciptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour //���ֿ� ���� �θ� ����
{

    public GameObject originPrefab;
    public UnitData data;
    //public NavMeshAgent agent { get; private set; }
    public UnitController controller { get; private set; }
    public AstarMover mover { get; private set; }
    public UnitDistinction distinction { get; private set; }
    public UnitDetectTarget detectTarget { get; private set; }
    public UnitAttackController attackController { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public CapsuleCollider2D capsuleCollider { get; private set; }
    public SpriteRenderer spriteRenderer;
    [HideInInspector] public PlayerController playerController;
    public Animator animator { get; private set; }

    protected virtual void Awake()
    {
        controller = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerController = FindAnyObjectByType<PlayerController>();
        distinction = GetComponentInChildren<UnitDistinction>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        attackController = GetComponent<UnitAttackController>();
        mover = GetComponent<AstarMover>();
        //agent = GetComponent<NavMeshAgent>();
    }
    public Unit GetUnit() { return this; }
    public UnitData GetData() { return data; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(originPrefab == null)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefab != null)
            {
                originPrefab = prefab;
                EditorUtility.SetDirty(this); // ������� ����
            }
        }
    }
#endif
}
