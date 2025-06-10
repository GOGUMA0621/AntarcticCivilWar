using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IGridScanner))]
public class FormationManger : SingleTonBehaviour<FormationManger>
{
    private IGridScanner gridScanner;

    private class FormationGroup
    {
        public List<AstarMover> units = new();
        public List<Vector3> assignedPositions = new();
        public AstarMover leader;
        public Vector2Int lastTargetGrid; // 추가: 마지막 타겟 그리드 위치
        public Transform targetTransform; // 추가: 타겟 트랜스폼
    }

    private Dictionary<Transform, FormationGroup> formationGroups = new();
    private Dictionary<Transform, Vector2Int> lastTargetGrid = new();
    private Dictionary<Transform, float> stopTimer = new();
    private float stopThreshold = 0.05f; // 멈췄다고 판단할 최소 이동 거리
    private float stopTime = 0.5f;       // 멈춘 상태로 간주할 시간(초)

    protected override void Awake()
    {
        base.Awake();
        gridScanner = GetComponent<IGridScanner>();
    }

    void Update()
    {
        foreach (var group in formationGroups.Values)
        {
            Transform target = group.targetTransform;
            Vector2Int currentGrid = gridScanner.WorldToGrid(target.position);

            // 이전 위치와 거리 계산
            if (!lastTargetGrid.TryGetValue(target, out var prevGrid))
                prevGrid = currentGrid;

            float dist = Vector3.Distance(target.position, gridScanner.GridToWorld(currentGrid));
            if (dist < stopThreshold)
                stopTimer[target] = stopTimer.TryGetValue(target, out var t) ? t + Time.deltaTime : Time.deltaTime;
            else
                stopTimer[target] = 0f;

            // "충분히 멈췄다"고 판단되면 타일 위치 비교
            if (stopTimer[target] >= stopTime)
            {
                if (currentGrid != prevGrid)
                {
                    lastTargetGrid[target] = currentGrid;
                    stopTimer[target] = 0f;
                    RequestFormationUpdate(target, group.leader);
                }
            }
        }
    }

    public void RegisterUnit(Transform targetTransform, AstarMover unit)
    {
        if (!formationGroups.ContainsKey(targetTransform))
            formationGroups[targetTransform] = new FormationGroup();

        var group = formationGroups[targetTransform];
        if (!group.units.Contains(unit))
            group.units.Add(unit);

        group.targetTransform = targetTransform; // 추가: 타겟 트랜스폼 업데이트
        UpdateLeader(group);
    }

    public void UnregisterUnit(Transform targetTransform, AstarMover unit)
    {
        if (!formationGroups.ContainsKey(targetTransform)) return;
        var group = formationGroups[targetTransform];
        group.units.Remove(unit);
        UpdateLeader(group);
    }

    public void ClearFormationGroup(Transform targetTransform)
    {
        if (formationGroups.ContainsKey(targetTransform))
            formationGroups.Remove(targetTransform);
    }

    private void UpdateLeader(FormationGroup group)
    {
        group.leader = group.units.Count > 0 ? group.units[0] : null;
    }

    /// <summary>
    /// 리더만 호출할 수 있는 포메이션 재계산 요청 메서드
    /// </summary>
    public void RequestFormationUpdate(Transform targetTransform, AstarMover requester)
    {
        if (!formationGroups.ContainsKey(targetTransform)) return;
        var group = formationGroups[targetTransform];
        if (group.leader != requester) return; // 리더만 호출 가능

        AssignFormation(targetTransform, group);
    }

    /// <summary>
    /// 실제 포메이션 좌표 재계산(내부 전용)
    /// </summary>
    private void AssignFormation(Transform targetTransform, FormationGroup group)
    {
        var units = group.units;
        if (units.Count == 0) return;

        Vector2Int centerGrid = gridScanner.WorldToGrid(targetTransform.position);

        var assignedGridPositions = new List<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(centerGrid);
        visited.Add(centerGrid);

        while (queue.Count > 0 && assignedGridPositions.Count < units.Count)
        {
            var current = queue.Dequeue();
            var currentNode = gridScanner.GetNode(current);
            if (gridScanner.HasNode(current) && currentNode.isWalkable)
                assignedGridPositions.Add(current);

            foreach (var neighbor in gridScanner.GetNeighbors(currentNode))
            {
                if (!visited.Contains(neighbor.gridPosition))
                {
                    visited.Add(neighbor.gridPosition);
                    var node = gridScanner.GetNode(neighbor.gridPosition);
                    if (node != null && node.isWalkable)
                    {
                        queue.Enqueue(neighbor.gridPosition);
                    }
                }
            }
        }

        group.assignedPositions.Clear();
        foreach (var gridPos in assignedGridPositions)
            group.assignedPositions.Add(gridScanner.GridToWorld(gridPos));

        for (int i = 0; i < units.Count; i++)
        {
            if (i < group.assignedPositions.Count)
                units[i].MoveTo(group.assignedPositions[i]);
            else
                units[i].MoveTo(targetTransform.position);
        }
    }

    /// <summary>
    /// 유닛이 자신의 포메이션 좌표를 요청할 때 사용하는 메서드
    /// </summary>
    public Vector3 GetAssignedPosition(Transform targetTransform, AstarMover unit)
    {
        if (!formationGroups.ContainsKey(targetTransform))
            return targetTransform.position;

        var group = formationGroups[targetTransform];
        int idx = group.units.IndexOf(unit);
        if (idx >= 0 && idx < group.assignedPositions.Count)
            return group.assignedPositions[idx];

        // 할당된 좌표가 없으면 타겟 위치 반환
        return targetTransform.position;
    }
}