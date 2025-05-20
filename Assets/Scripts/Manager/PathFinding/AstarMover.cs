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

    private List<Vector3> worldPath = new();
    private int currentIndex = 0;
    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;
    private Action onPathComplete;

    /// <summary>
    /// 목적지(월드 좌표)만 받아 이동 시작
    /// </summary>
    public void MoveTo(Vector3 worldDestination, Action onComplete = null)
    {
        isMoving = false;
        worldPath.Clear();
        currentIndex = 0;
        velocity = Vector3.zero;
        onPathComplete = onComplete;

        // 자동으로 GridManager의 변환 함수 사용

        // AstarPathFinding.instance.RequestPath(startGrid, endGrid, OnPathFound);
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

        currentIndex = 0;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving || worldPath == null || currentIndex >= worldPath.Count)
            return;

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

        // 실제 이동
        transform.position += velocity * Time.deltaTime;

        // 웨이포인트 도달 판정
        if (distance < waypointTolerance)
        {
            currentIndex++;
            if (currentIndex >= worldPath.Count)
            {
                isMoving = false;
                velocity = Vector3.zero;
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
