using System.Collections.Generic;
using DG.Tweening;
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

            if (!lastTargetGrid.TryGetValue(target, out var prevGrid) || currentGrid != prevGrid)
            {
                lastTargetGrid[target] = currentGrid;
                RequestFormationUpdate(target, group.leader);
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

        // 목표 주변에서 유닛 수만큼의 인접 타일을 BFS로 탐색
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
            {
                // 이미 다른 유닛이 목표로 하는 타일은 제외
                if (!assignedGridPositions.Contains(current))
                    assignedGridPositions.Add(current);
            }

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

    }

    /// <summary>
    /// 유닛이 자신의 포메이션 좌표를 요청할 때 사용하는 메서드
    /// </summary>
    public Vector3 GetAssignedPosition(Transform targetTransform, AstarMover unit)
    {
        if (!formationGroups.ContainsKey(targetTransform))
            return targetTransform.position;

        // 목표가 유닛일 경우
        if (targetTransform.TryGetComponent<UnitController>(out var targetUnit))
        {
            float attackRange = unit.GetAttackRange();

            if (attackRange <= 1.01f) // 근접 유닛
            {
                // 목표 주변 8방향 좌표 중 이동 가능한 곳 반환
                Vector2Int center = gridScanner.WorldToGrid(targetTransform.position);
                foreach (var offset in GetNeighborOffsets())
                {
                    Vector2Int neighbor = center + offset;
                    var node = gridScanner.GetNode(neighbor);
                    if (node != null && node.isWalkable)
                    {
                        Debug.Log($"근접 유닛이 이동 가능한 위치: {gridScanner.GridToWorld(neighbor)}");
                        return gridScanner.GridToWorld(neighbor);
                    }
                }
                // 못 찾으면 목표 위치 반환
                Debug.LogWarning("근접 유닛이 이동 가능한 위치를 찾지 못했습니다. 기본 위치 반환.");
                return targetTransform.position;
            }
            else
            {
                // 원거리 유닛은 기존 방식
                Vector3 dir = (targetTransform.position - unit.transform.position).normalized;
                Vector3 attackPos = targetTransform.position - dir * attackRange;
                return attackPos;
            }
        }

        // 기존 포메이션 방식
        var group = formationGroups[targetTransform];
        int idx = group.units.IndexOf(unit);
        if (idx >= 0 && idx < group.assignedPositions.Count)
            return group.assignedPositions[idx];

        return targetTransform.position;
    }

    // 8방향 오프셋
    private static readonly Vector2Int[] neighborOffsets = new Vector2Int[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)
    };

    private IEnumerable<Vector2Int> GetNeighborOffsets() => neighborOffsets;
}