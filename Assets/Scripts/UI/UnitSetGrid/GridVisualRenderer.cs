using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualRenderer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;
    public Material lineMaterial;

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        for (int x = 0; x <= width; x++)
        {
            CreateLine(
                origin + new Vector3(x * cellSize, 0, 0),
                origin + new Vector3(x * cellSize, height * cellSize, 0)
            );
        }

        for (int y = 0; y <= height; y++)
        {
            CreateLine(
                origin + new Vector3(0, y * cellSize, 0),
                origin + new Vector3(width * cellSize, y * cellSize, 0)
            );
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine", typeof(LineRenderer));
        lineObj.transform.parent = this.transform;

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.05f;
        lr.startColor = Color.green;
        lr.endColor = Color.green;
        lr.sortingOrder = -1;
    }
}
