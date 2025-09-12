using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridScanner
{
    Vector3 origin { get; }
    float cellSize { get; }

/// <summary>
/// 맵 전체를 스캔하여 노드 정보를 생성합니다.
/// 이 메서드는 맵의 크기와 장애물 정보를 기반으로 노드를 초기화합니다.
/// 노드 정보는 월드 좌표와 그리드 좌표를 포함하며, 각 노드의 이동 가능 여부를 판단합니다.
/// 노드 정보는 A* 경로 탐색 알고리즘에서 사용됩니다.
/// </summary>
    void Scan(); // 맵 전체를 스캔해서 노드 정보 생성
    /// <summary>
    /// 특정 위치에 노드가 존재하는지 확인합니다.
    /// 이 메서드는 주어진 그리드 좌표에 노드가 있는지 여부를 반환합니다.
    /// </summary>
    /// <param name="pos">노드의 그리드 좌표</param>
    /// <returns>노드의 여부 반환</returns>
    bool HasNode(Vector2Int pos);
    Node GetNode(Vector2Int pos);
    IEnumerable<Node> GetAllNodes();
    List<Node> GetNeighbors(Node node);

    Vector2Int WorldToGrid(Vector3 worldPos);
    Vector3 GridToWorld(Vector2Int gridPos);
}
