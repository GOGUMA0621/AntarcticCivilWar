using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(UnitController))]
public class AstarMover : MonoBehaviour
{
    public static HashSet<Vector2Int> AllUnitGridPositions = new HashSet<Vector2Int>();

    [Header("이동 설정")]
    public float maxSpeed = 3f;
    public float acceleration = 10f;
    public float slowdownDistance = 0.5f;
    public float waypointTolerance = 0.1f;
    public float stopDistance = 1.0f;

    private List<Vector3> worldPath = new();
    private int currentIndex = 0;
    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private bool canMove = true; // 이동 가능 여부
    private Action onPathComplete;

    private IGridScanner gridScanner;
    private Rigidbody2D rb;

    // 포메이션 관련
    private Transform formationTarget; // 목표 Transform(타겟)
    private Vector3 assignedFormationPosition; // 포메이션 매니저가 할당한 위치

    private bool isWaitingForPath = false;
    private float retryTimer = 0f;
    private float retryInterval = 1f;
    private Transform targetTransform;
    private Vector2Int lastTargetGridPos;

    private Vector2Int lastGridPos;

    private int UnitLayerMask => LayerMask.GetMask("Unit");

    private FormationManger formationManager => FormationManger.instance;

    [HideInInspector] public UnitController unitController;

    private void OnDisable()
    {
        AllUnitGridPositions.Remove(lastGridPos);
    }

    private void Awake()
    {
        unitController = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        if (gridScanner == null)
        {
            var scanner = FindObjectOfType<AstarPathfinder>();
            gridScanner = scanner is IGridScanner ? scanner : null;
            if (gridScanner == null)
            {
                Debug.LogError("AstarPathFinding requires a GridScanner to function. Please assign one in the inspector or ensure it is present in the scene.");
            }
        }
    }
    void Update()
    {
        if (!canMove)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        Vector2Int currentGridPos = gridScanner.WorldToGrid(transform.position);
        if( lastGridPos != currentGridPos)
        {
            AllUnitGridPositions.Remove(lastGridPos);
            AllUnitGridPositions.Add(currentGridPos);
            lastGridPos = currentGridPos;
        }

        // 경로 요청 실패 시 재시도 타이머 로직 (기존 유지)
        if (isWaitingForPath)
        {
            retryTimer += Time.deltaTime;
            if (retryTimer >= retryInterval)
            {
                retryTimer = 0f;
                isWaitingForPath = false;
                // 다음 업데이트에서 새 요청 허용
            }
        }

        // 이하 기존 이동 코드
        if (!isMoving || worldPath == null || currentIndex >= worldPath.Count)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        Vector3 target = worldPath[currentIndex];
        Vector3 toTarget = target - transform.position;
        float distance = toTarget.magnitude;

        // 마지막 웨이포인트에서 감속
        float speed = maxSpeed;
        if (currentIndex == worldPath.Count - 1 && distance < slowdownDistance)
            speed = Mathf.Lerp(0, maxSpeed, distance / slowdownDistance);

        // 가속도 적용
        Vector3 desiredVelocity = toTarget.normalized * speed;
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime);

        // Rigidbody2D에 velocity 적용 (z축 무시)
        if (rb != null)
            rb.velocity = new Vector2(velocity.x, velocity.y);

        // 이동 중 앞에 유닛이 있으면 속도 줄이기/멈추기
        var hits = Physics2D.OverlapCircleAll(transform.position + velocity.normalized * 0.2f, 0.2f, UnitLayerMask);
        foreach (var hit in hits)
        {
            if (hit.transform == targetTransform)
            {
                OnPathEnd();
                Debug.Log($"타겟과 충돌: {hit.transform.name}");
                break;
            }
            if (hit.gameObject != this.gameObject)
            {
                // 앞에 유닛이 있으면 현재 타겟(목적지)로 재요청
                MoveTo(targetTransform != null ? targetTransform.position : worldPath[^1]);
                break;
            }
        }

        // 웨이포인트 도달 판정
        if (distance < waypointTolerance)
        {
            // 목표 그리드에 도달했는지 확인
            Vector2Int currGrid = gridScanner.WorldToGrid(transform.position);
            if (currGrid == lastTargetGridPos)
            {
                // 최종 목표 도달
                isMoving = false;
                velocity = Vector3.zero;
                if (rb != null) rb.velocity = Vector2.zero;
                onPathComplete?.Invoke();
                onPathComplete = null;
                Debug.Log($"목표 그리드에 도달했습니다. {this.gameObject.name} grid:{currGrid}");
                return;
            }

            // 이미 경로 재요청 중이면 기다림
            if (isWaitingForPath)
            {
                // 멈추지 않고 현재 velocity 유지하거나 감속 처리 가능 (여기선 멈추지 않음)
                return;
            }

            // 한 칸(웨이포인트)마다 경로 재계산 요청
            Vector2Int startGrid = gridScanner.WorldToGrid(transform.position);
            isWaitingForPath = true;
            retryTimer = 0f;
            AstarPathFinding.instance.RequestPath(
                startGrid,
                lastTargetGridPos,
                this.gameObject,
                OnPathFound
            );

            // 새 경로 올 때까지 기존 이동 경로는 중단
            isMoving = false;
            velocity = Vector3.zero;
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }
    }

    /// <summary>
    /// 포메이션 매니저가 할당한 위치로 이동
    /// </summary>
    public void FollowTarget(Transform target, Action onCompleted = null)
    {
        formationTarget = target;
        targetTransform = target;
        // 포메이션 매니저에 등록
        FormationManger.instance.RegisterUnit(target, this);

        // 내 좌표 받아오기
        assignedFormationPosition = FormationManger.instance.GetAssignedPosition(target, this);

        // 이동 시작
        MoveTo(assignedFormationPosition, onCompleted);
    }

    /// <summary>
    /// 포메이션 해제(개별 행동 등)
    /// </summary>
    public void ClearFormation()
    {
        formationTarget = null;
    }

    /// <summary>
    /// 목적지(월드 좌표)만 받아 이동 시작
    /// </summary>
    public void MoveTo(Vector3 worldDestination, Action onComplete = null, bool clearAction = false)
    {
        // 목적지가 현재와 거의 같으면 무시
        if (worldPath.Count > 0 && Vector3.Distance(worldPath[^1], worldDestination) < 0.1f)
            return;

        isMoving = false;
        worldPath.Clear();
        currentIndex = 0;
        if( onComplete == null && clearAction)
        {
            onPathComplete = null; // 기존 콜백 제거
        }
        else if (onComplete != null)
        {
           onPathComplete = onComplete; // 새로운 콜백 설정
        }

        // 목표 그리드 저장 (한 칸마다 재계산할 최종 목표)
        lastTargetGridPos = gridScanner.WorldToGrid(worldDestination);

        // --- 추가: 현재 씬에 점유된 그리드 집합을 복사하고 요청자(자기 자신)의 현재 칸은 제외 ---
        var occupied = new HashSet<Vector2Int>(AllUnitGridPositions);
        // 현재 위치(요청자) 칸은 예외로 허용 (시작 칸이 막혀 길 생성 불가해지는 것을 방지)
        var startGrid = gridScanner.WorldToGrid(this.transform.position);
        occupied.Remove(startGrid);
        // 목표칸도 허용(타겟에 도달 가능하게)
        occupied.Remove(lastTargetGridPos);

        AstarPathFinding.instance.RequestPath(
            startGrid,
            lastTargetGridPos,
            this.gameObject,
            OnPathFound,
            occupied // 새로 추가된 파라미터
        );
    }
    /// <summary>
    /// 경로 탐색 완료 콜백
    /// </summary>
    /// <param name="path">탐색된 경로</param>
    private void OnPathFound(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0) // 경로 없음
        {
            isMoving = false;
            isWaitingForPath = true;
            retryTimer = 0f;
            onPathComplete?.Invoke();
            return;
        }

        worldPath.Clear();
        foreach (var gridPos in path) // 그리드 좌표를 월드 좌표로 변환
        {
            worldPath.Add(gridScanner.GridToWorld(gridPos));
        }
        currentIndex = 0;
        isMoving = true;
        isWaitingForPath = false;
    }
    /// <summary>
    /// 경로 도착 처리
    /// </summary>
    private void OnPathEnd()
    {
        isMoving = false;
        velocity = Vector3.zero;
        if (rb != null) rb.velocity = Vector2.zero;
        onPathComplete?.Invoke();
        onPathComplete = null; // 콜백 초기화
    }

    // 타겟을 transform으로 지정
    public void FollowTargetSingle(Transform target, Action onComplete = null)
    {
        targetTransform = target;
        lastTargetGridPos = gridScanner.WorldToGrid(target.position);
        ClearFormation(); // 포메이션 해제
        MoveTo(target.position, onComplete);
    }

    public void ClearTarget()
    {
        targetTransform = null;
        lastTargetGridPos = Vector2Int.zero;
        isMoving = false;
        worldPath.Clear();
        currentIndex = 0;
        velocity = Vector3.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }


    /// <summary>
    /// 즉시 위치 이동(텔레포트)
    /// </summary>
    public void Teleport(Vector3 position, bool clearPath = true)
    {
        Vector2Int gridPos = gridScanner?.WorldToGrid(position) ?? Vector2Int.zero;
        Vector3 worldPos = gridScanner?.GridToWorld(gridPos) ?? position;

        transform.position = worldPos;
        velocity = Vector3.zero;
        if (clearPath)
        {
            isMoving = false;
            worldPath.Clear();
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if( !canMove && rb != null)
        {
            rb.velocity = Vector2.zero; // 이동 불가 시 속도 초기화
        }
    }

    /// <summary>
    /// 남은 경로 반환
    /// </summary>
    public List<Vector3> GetRemainingPath()
    {
        if (!isMoving || worldPath == null || currentIndex >= worldPath.Count) return new List<Vector3>();
        return worldPath.GetRange(currentIndex, worldPath.Count - currentIndex);
    }

    /// <summary>
    /// 남은 경로의 칸 수 반환
    /// </summary>
    public int GetRemainingGridDistance()
    {
        if (worldPath == null || currentIndex >= worldPath.Count)
            return 0;
        return worldPath.Count - currentIndex;
    }

    /// <summary>
    /// 남은 경로의 월드 거리 반환
    /// </summary>
    public float GetRemainingWorldDistance()
    {
        if (!isMoving || worldPath == null || currentIndex >= worldPath.Count)
            return 0f;

        float dist = 0f;
        Vector3 prev = transform.position;
        for (int i = currentIndex; i < worldPath.Count; i++)
        {
            dist += Vector3.Distance(prev, worldPath[i]);
            prev = worldPath[i];
        }
        return dist;
    }

    public int GetRemainingTileDistanceToTarget()
    {
        if (formationTarget == null)
        {
            Debug.LogWarning("타겟이 설정되지 않았습니다. 이동할 타겟을 설정해주세요.");
            return 0;
        }
        Vector2Int currentGridPos = gridScanner.WorldToGrid(transform.position);
        Vector2Int targetGridPos = gridScanner.WorldToGrid(formationTarget.position);
        // Debug.Log($"현재 위치: {currentGridPos}, 타겟 위치: {targetGridPos}");
        if (currentGridPos == targetGridPos)
            return 0;

        int dx = Mathf.Abs(currentGridPos.x - targetGridPos.x);
        int dy = Mathf.Abs(currentGridPos.y - targetGridPos.y);
        return Mathf.Max(dx, dy); // 대각선 포함 최소 칸 수
    }

    public float GetAttackRange()
    {
        return unitController.UnitStats.attackRange;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (worldPath == null || worldPath.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < worldPath.Count - 1; i++)
        {
            Gizmos.DrawLine(worldPath[i], worldPath[i + 1]);
            Gizmos.DrawSphere(worldPath[i], 0.05f);
        }

        // 현재 위치와 다음 웨이포인트 표시
        if (currentIndex < worldPath.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldPath[currentIndex], 0.1f);
        }
    }
#endif
}
