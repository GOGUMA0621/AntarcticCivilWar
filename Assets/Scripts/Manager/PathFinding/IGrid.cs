using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridScanner
{
    void Scan(); // 맵 전체를 스캔해서 노드 정보 생성
    bool HasNode(Vector2Int pos);
    Node GetNode(Vector2Int pos);
    IEnumerable<Node> GetAllNodes();
    List<Node> GetNeighbors(Node node);

    Vector2Int WorldToGrid(Vector3 worldPos);
    Vector3 GridToWorld(Vector2Int gridPos);
}
