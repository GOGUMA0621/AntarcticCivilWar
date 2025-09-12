#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Unity.Mathematics;

[CustomEditor(typeof(AstarPathfinder))]
public class AstarPathfinderEditor : Editor
{
    private const float handleSize = 0.1f;

    void OnSceneGUI()
    {
        AstarPathfinder pf = (AstarPathfinder)target;

        // 기본값 설정
        if (pf.boundsSize == Vector2.zero)
            pf.boundsSize = new Vector2(pf.width * pf.nodeSize, pf.height * pf.nodeSize);
        if (pf.boundsCenter == Vector2.zero)
            pf.boundsCenter = new Vector2(pf.center.x, pf.center.y);

        Vector2 center = pf.boundsCenter;
        Vector2 size = pf.boundsSize;
        float snapUnit = pf.nodeSize * 0.5f;

        // 3D 변환 (XY 평면, z=0)
        Vector3 center3D = new Vector3(center.x, center.y, 0);
        Vector3 halfSize = new Vector3(size.x / 2f, size.y / 2f, 0);

        Vector3 left = center3D + new Vector3(-halfSize.x, 0, 0);
        Vector3 right = center3D + new Vector3(halfSize.x, 0, 0);
        Vector3 top = center3D + new Vector3(0, halfSize.y, 0);
        Vector3 bottom = center3D + new Vector3(0, -halfSize.y, 0);

        Handles.color = Color.yellow;

        EditorGUI.BeginChangeCheck();

        int controlIDLeft = GUIUtility.GetControlID(FocusType.Passive);
        int controlIDRight = GUIUtility.GetControlID(FocusType.Passive);
        int controlIDTop = GUIUtility.GetControlID(FocusType.Passive);
        int controlIDBottom = GUIUtility.GetControlID(FocusType.Passive);

        // 네 변 핸들
        Vector3 newLeft = Handles.FreeMoveHandle(controlIDLeft,left, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newRight = Handles.FreeMoveHandle(controlIDRight,right, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newTop = Handles.FreeMoveHandle(controlIDTop ,top, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newBottom = Handles.FreeMoveHandle(controlIDBottom , bottom, handleSize, Vector3.zero, Handles.DotHandleCap);

        // 센터 핸들
        Vector3 newCenter3D = Handles.PositionHandle(center3D, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pf, "Resize or Move Grid");

            // 센터 이동
            if (newCenter3D != center3D)
            {
                Vector2 snappedCenter = SnapPosition(new Vector2(newCenter3D.x, newCenter3D.y), snapUnit);
                pf.boundsCenter = snappedCenter;
            }
            else
            {
                // X축
                if (newLeft != left)
                {
                    float rightEdge = center.x + size.x / 2f;
                    float newLeftEdge = newLeft.x;
                    size.x = Mathf.Max(snapUnit, rightEdge - newLeftEdge);
                    center.x = (rightEdge + newLeftEdge) / 2f;
                }
                else if (newRight != right)
                {
                    float leftEdge = center.x - size.x / 2f;
                    float newRightEdge = newRight.x;
                    size.x = Mathf.Max(snapUnit, newRightEdge - leftEdge);
                    center.x = (leftEdge + newRightEdge) / 2f;
                }

                // Y축
                if (newTop != top)
                {
                    float bottomEdge = center.y - size.y / 2f;
                    float newTopEdge = newTop.y;
                    size.y = Mathf.Max(snapUnit, newTopEdge - bottomEdge);
                    center.y = (bottomEdge + newTopEdge) / 2f;
                }
                else if (newBottom != bottom)
                {
                    float topEdge = center.y + size.y / 2f;
                    float newBottomEdge = newBottom.y;
                    size.y = Mathf.Max(snapUnit, topEdge - newBottomEdge);
                    center.y = (topEdge + newBottomEdge) / 2f;
                }

                // 스냅 적용
                size.x = Mathf.Max(snapUnit, Mathf.Round(size.x / snapUnit) * snapUnit);
                size.y = Mathf.Max(snapUnit, Mathf.Round(size.y / snapUnit) * snapUnit);
                center.x = Mathf.Round(center.x / (snapUnit * 0.5f)) * (snapUnit * 0.5f);
                center.y = Mathf.Round(center.y / (snapUnit * 0.5f)) * (snapUnit * 0.5f);

                pf.boundsSize = size;
                pf.boundsCenter = center;
            }

            // pf.center, width, height 갱신
            pf.center = new Vector3(pf.boundsCenter.x, pf.boundsCenter.y, 0);
            pf.width = Mathf.Max(1, Mathf.RoundToInt(pf.boundsSize.x / pf.nodeSize));
            pf.height = Mathf.Max(1, Mathf.RoundToInt(pf.boundsSize.y / pf.nodeSize));
            EditorUtility.SetDirty(pf);
        }

        // 시각화
        Handles.color = Color.cyan;
        Handles.DrawWireCube(pf.boundsCenter, new Vector3(pf.boundsSize.x, pf.boundsSize.y, 0.1f));

        // --- 노드 시각화 ---
        if (pf.grid != null && pf.width == pf.grid.GetLength(0) && pf.height == pf.grid.GetLength(1))
        {
            Handles.color = new Color(0, 1, 0, 0.2f); // 연한 초록색
            float nodeSize = pf.nodeSize;

            for (int x = 0; x < pf.width; x++)
            {
                for (int y = 0; y < pf.height; y++)
                {
                    Node node = pf.GetNode(new Vector2Int(x, y));
                    if (node == null) continue;

                    Vector3 pos = node.worldPosition;
                    // XY 평면 기준 사각형 꼭짓점
                    Vector3[] verts = new Vector3[4]
                    {
                        pos + new Vector3(-nodeSize/2, -nodeSize/2, 0),
                        pos + new Vector3(-nodeSize/2,  nodeSize/2, 0),
                        pos + new Vector3( nodeSize/2,  nodeSize/2, 0),
                        pos + new Vector3( nodeSize/2, -nodeSize/2, 0)
                    };
                    if(node.isWalkable)
                    {
                        Handles.DrawSolidRectangleWithOutline(verts, new Color(0,1,0,0.15f), Color.green);
                    }
                    else
                    {
                        Handles.DrawSolidRectangleWithOutline(verts, new Color(1,0,0,0.15f), Color.red);
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AstarPathfinder pf = (AstarPathfinder)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Scan Grid"))
        {
            pf.Scan();
            SceneView.RepaintAll();
        }
    }

    private float SnapSizeFloat(float value, float snap)
{
    return Mathf.Max(snap, Mathf.Round(value / snap) * snap); // 크기는 최소 snap
}

private float SnapPositionFloat(float value, float snap)
{
    return Mathf.Round(value / snap) * snap; // 위치는 음수 가능
}

private Vector2 SnapSize(Vector2 v, float snap)
{
    return new Vector2(SnapSizeFloat(v.x, snap), SnapSizeFloat(v.y, snap));
}

private Vector2 SnapPosition(Vector2 v, float snap)
{
    return new Vector2(SnapPositionFloat(v.x, snap), SnapPositionFloat(v.y, snap));
}
}
#endif
