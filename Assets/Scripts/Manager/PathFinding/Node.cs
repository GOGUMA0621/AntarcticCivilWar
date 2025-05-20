using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 경로를 저장하기 위한 노드 클래스입니다.
/// A* 알고리즘에서 사용됩니다.
/// 노드는 그리드 상의 위치와 이동 가능 여부, gCost, hCost, fCost를 저장합니다.
/// gCost는 시작 노드에서 현재 노드까지의 비용, hCost는 현재 노드에서 목표 노드까지의 비용입니다.
/// fCost는 gCost와 hCost의 합입니다.
/// fCost는 A* 알고리즘에서 가장 낮은 값을 가진 노드를 선택하는 데 사용됩니다.
/// 노드는 부모 노드를 가지고 있으며, 부모 노드는 현재 노드의 경로를 추적하는 데 사용됩니다.
/// 노드는 그리드 상의 위치와 이동 가능 여부를 저장합니다.
/// </summary>  
public class Node
{
    /// <summary>
    /// 그리드 상의 위치를 저장합니다.
    /// </summary>
    public Vector2Int gridPosition;
    /// <summary>
    /// 이동 가능 여부를 저장합니다.
    /// </summary>
    public Vector3 worldPosition;
    public bool isWalkable;
    /// <summary>
    /// 출발 노드에서 현재 노드까지의 비용을 저장합니다.
    /// </summary>
    public int gCost;
    /// <summary>
    /// 현재 노드에서 목표 노드까지의 비용을 저장합니다.
    /// </summary>
    public int hCost;
    /// <summary>
    /// fCost는 gCost와 hCost의 합입니다.
    /// fCost는 A* 알고리즘에서 가장 낮은 값을 가진 노드를 선택하는 데 사용됩니다.
    /// </summary>
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    /// <summary>
    /// 부모 노드를 저장합니다.
    /// 부모 노드는 현재 노드의 경로를 추적하는 데 사용됩니다.
    /// </summary>
    /// <returns>부모 노드는 현재 노드의 경로를 추적하는 데 사용됩니다.</returns>
    public Node parentNode;
    /// <summary>
    /// 노드 생성자입니다.
    /// </summary>
    /// <param name="gridPosition">그리드 상의 위치를 저장합니다.</param>
    /// <param name="isWalkable">이동 가능 여부를 저장합니다.</param>
    public Node(Vector2Int gridPosition, Vector3 worldPosition, bool isWalkable)
    {
        this.gridPosition = gridPosition;
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
    }
}
