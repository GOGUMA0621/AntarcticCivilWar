using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 목적지 Vector3(월드 좌표)만 받아서, 자동으로 그리드 변환 및 경로 요청 후 이동.
/// 변환 함수 할당 필요 없음.
/// </summary>
public class AstarMover : MonoBehaviour
{
    [Header("이동 설정")]
    public float maxSpeed = 3f;
    public float acceleration = 10f;
    public float slowdownDistance = 0.5f;
    public float waypointTolerance = 0.1f;
    public float stopDistance = 1.0f; // 유닛 사거리(또는 공격 사거리) 값

    private AstarPathFinding pathFinding; // A* 경로 탐색기 인스턴스

    private List<Vector3> worldPath = new();
    private int currentIndex = 0;
    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private Action onPathComplete;

    private IGridScanner gridScanner;
    private Rigidbody2D rb;

    private Transform targetTransform;
    private Vector2Int lastTargetGridPos;
    private float repathInterval = 0.2f;
    private float repathTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        var pathfinder = FindObjectOfType<AstarPathfinder>();
        gridScanner = pathfinder is IGridScanner ? pathfinder : null;
        Debug.Log($"GridScanner found: {gridScanner}");

        pathFinding = new AstarPathFinding(gridScanner);
    }

    /// <summary>
    /// 목적지(월드 좌표)만 받아 이동 시작
    /// </summary>
    public void MoveTo(Vector3 worldDestination, Action onComplete = null)
    {
        isMoving = false;
        worldPath.Clear();
        currentIndex = 0;
        velocity = Vector3.zero;
        onPathComplete += onComplete;

        pathFinding.RequestPath(
            gridScanner.WorldToGrid(transform.position),
            gridScanner.WorldToGrid(worldDestination),
            OnPathFound
        );
    }

    private void OnPathFound(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
        {
            isMoving = false;
            onPathComplete?.Invoke();
            return;
        }

        worldPath.Clear();
        foreach (var gridPos in path)
        {
            // 그리드 좌표를 월드 좌표로 변환하여 경로에 추가
            worldPath.Add(gridScanner.GridToWorld(gridPos));
        }
        // Debug.Log($"Path found with {worldPath.Count} waypoints.");
        currentIndex = 0;
        isMoving = true;
    }

    // 타겟을 transform으로 지정
    public void FollowTarget(Transform target, Action onComplete = null)
    {
        targetTransform = target;
        lastTargetGridPos = gridScanner.WorldToGrid(target.position);
        MoveTo(target.position, onComplete);
    }

    public void ClearTarget()
    {
        targetTransform = null;
        lastTargetGridPos = Vector2Int.zero;
        repathTimer = 0f;
        isMoving = false;
        worldPath.Clear();
        currentIndex = 0;
        velocity = Vector3.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // 타겟 추적 모드일 때
        if (targetTransform != null)
        {
            // --- 칸 단위 거리 계산 ---
            Vector2Int myGrid = gridScanner.WorldToGrid(transform.position);
            Vector2Int targetGrid = gridScanner.WorldToGrid(targetTransform.position);
            int gridDistance = Mathf.Abs(myGrid.x - targetGrid.x) + Mathf.Abs(myGrid.y - targetGrid.y); // 맨해튼 거리

            if (gridDistance <= stopDistance)
            {
                // 사거리(칸) 안에 들어오면 이동 중지 및 경로 재계산 중단
                isMoving = false;
                if (rb != null) rb.velocity = Vector2.zero;
                return;
            }

            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval)
            {
                repathTimer = 0f;
                Vector2Int currentTargetGrid = gridScanner.WorldToGrid(targetTransform.position);
                if (currentTargetGrid != lastTargetGridPos)
                {
                    lastTargetGridPos = currentTargetGrid;
                    MoveTo(targetTransform.position); // 경로 재요청
                }
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

        // 웨이포인트 도달 판정
        if (distance < waypointTolerance)
        {
            currentIndex++;
            if (currentIndex >= worldPath.Count)
            {
                isMoving = false;
                velocity = Vector3.zero;
                if (rb != null) rb.velocity = Vector2.zero;
                onPathComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// 즉시 위치 이동(텔레포트)
    /// </summary>
    public void Teleport(Vector3 position, bool clearPath = true)
    {
        transform.position = position;
        velocity = Vector3.zero;
        if (clearPath)
        {
            isMoving = false;
            worldPath.Clear();
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
        if (!isMoving || worldPath == null || currentIndex >= worldPath.Count)
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

    // --- 내부 변환 함수 ---
    // private Vector2Int WorldToGrid(Vector3 worldPos)
    // {
    //     // GridManager 또는 TilemapManager의 변환 함수 사용
    //     // 필요에 따라 아래 한 줄만 수정하세요.
    //     return ;
    //     // 또는: return TilemapManager.instance.WorldToGrid(worldPos);
    // }

    // private Vector3 GridToWorld(Vector2Int gridPos)
    // {
    //     // GridManager 또는 TilemapManager의 변환 함수 사용
    //     // 필요에 따라 아래 한 줄만 수정하세요.
    //     return ;
    //     // 또는: return TilemapManager.instance.GridToWorld(gridPos);
    // }
}
