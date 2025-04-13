#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour //유닛에 대한 부모 파일
{

    public GameObject originPrefab;
    protected UnitController controller;
    public UnitController unitController { get { return controller; } }
    protected UnitDistinction distinction;
    public UnitDistinction unitDistinction { get { return distinction; } }
    protected UnitDetectTarget detectTarget;
    public UnitDetectTarget unitDetectTarget { get { return detectTarget; } }
    protected UnitAttackController attackController;
    public UnitAttackController unitAttackController { get { return attackController; } }
    public SciptableObjects.UnitData data;

    protected Rigidbody2D rb;
    protected CircleCollider2D circleCollider;
    protected SpriteRenderer spriteRenderer;
    [HideInInspector] public PlayerController playerController;
    protected PlayerUnitManager playerUnitManager;
    protected Animator animator;
    public Animator unitAnimator {  get { return animator; } }

    protected virtual void Start()
    {
        controller = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerController = GameObject.FindAnyObjectByType<PlayerController>();
        distinction = GetComponentInChildren<UnitDistinction>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        attackController = GetComponent<UnitAttackController>();
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
