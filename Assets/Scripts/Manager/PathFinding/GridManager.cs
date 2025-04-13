using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : SingleTonBehaviour<GridManager>
{
    private Tilemap groundTilemap; 
    private Dictionary<Vector2Int, Node> grid = new Dictionary<Vector2Int, Node>(); //노드의 위치를 키로 사용하여 노드를 저장하는 딕셔너리

    private void Start()
    {
        if(TilemapManager.instance != null)
        {
           groundTilemap = TilemapManager.instance.tilemap; //타일맵 매니저에서 타일맵을 가져옴
        }
        // 그리드 초기화
        StartCoroutine(GenerateGridAfterInit());
    }

    private IEnumerator GenerateGridAfterInit()
    {
        yield return null; // 다음 프레임까지 대기
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Vector2Int gridPosition = new Vector2Int(x, y);

                bool isWalkable = TilemapManager.instance.IsValidPoint(tilePos);
                grid[gridPosition] = new Node(gridPosition, isWalkable);
            }
        }
    }

    /// <summary>
    /// 외부에서 노드에 접근할 수 있도록 하는 메서드
    /// </summary>

    public Node GetNode(Vector2Int position) //그리드 좌표에 해당하는 노드를 반환하는 메서드
    {
        return grid.ContainsKey(position) ? grid[position] : null; 
    }

    public bool HasNode(Vector2Int position) //그리드에 해당 좌표의 노드가 존재하는지 확인하는 메서드
    {
        return grid.ContainsKey(position);
    }

    /// <summary>
    /// (선택) 월드 좌표 <-> 그리드 좌표 변환
    /// </summary>
    
    public Vector2Int WorldToGrid(Vector3 worldPosition) //월드 좌표를 그리드 좌표로 변환
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(worldPosition); 
        return new Vector2Int(cellPosition.x, cellPosition.y); 
    }

    public Vector3 GridToWorld(Vector2Int gridPosition) //그리드 좌표를 월드 좌표로 변환
    {
        Vector3Int cellPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        return groundTilemap.GetCellCenterWorld(cellPosition);
    }
}
