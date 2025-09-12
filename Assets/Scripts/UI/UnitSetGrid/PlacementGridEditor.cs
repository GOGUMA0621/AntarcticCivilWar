#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlacementGridManager))]
public class PlacementGridEditor : Editor
{
    private const float handleSize = 0.1f;

    void OnSceneGUI()
    {
        PlacementGridManager pf = (PlacementGridManager)target;

        // 기본값 보정
        if (pf.width < 1) pf.width = 1;
        if (pf.height < 1) pf.height = 1;
        if (pf.cellSize <= 0) pf.cellSize = 1f;

        Vector3 origin = pf.origin;
        float cellSize = pf.cellSize;
        int width = pf.width;
        int height = pf.height;

        // 네 변 핸들 위치 계산 (XY 평면)
        Vector3 left   = origin + new Vector3(0, height * cellSize / 2f, 0);
        Vector3 right  = origin + new Vector3(width * cellSize, height * cellSize / 2f, 0);
        Vector3 top    = origin + new Vector3(width * cellSize / 2f, height * cellSize, 0);
        Vector3 bottom = origin + new Vector3(width * cellSize / 2f, 0, 0);

        // 중앙 핸들
        Vector3 center = origin + new Vector3(width * cellSize / 2f, height * cellSize / 2f, 0);

        EditorGUI.BeginChangeCheck();

        // 네 변 핸들 (스냅 없이)
        Vector3 newLeft   = Handles.FreeMoveHandle(left, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newRight  = Handles.FreeMoveHandle(right, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newTop    = Handles.FreeMoveHandle(top, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 newBottom = Handles.FreeMoveHandle(bottom, handleSize, Vector3.zero, Handles.DotHandleCap);

        // 중앙 핸들 (스냅 없이)
        Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pf, "Resize or Move Placement Grid");

            // 중앙 이동
            if (newCenter != center)
            {
                // 마지막에만 스냅 적용
                Vector3 snappedCenter = SnapPosition(newCenter, cellSize * 0.5f);
                pf.origin = snappedCenter - new Vector3(width * cellSize / 2f, height * cellSize / 2f, 0);
            }
            else
            {
                // 왼쪽 변 이동
                if (newLeft != left)
                {
                    float rightEdge = origin.x + width * cellSize;
                    float newLeftEdge = Mathf.Min(newLeft.x, rightEdge - cellSize); // 최소 1칸 유지
                    // 마지막에만 스냅 적용
                    newLeftEdge = Mathf.Round(newLeftEdge / cellSize) * cellSize;
                    int newWidth = Mathf.Max(1, Mathf.RoundToInt((rightEdge - newLeftEdge) / cellSize));
                    pf.origin.x = newLeftEdge;
                    pf.width = newWidth;
                }
                // 오른쪽 변 이동
                else if (newRight != right)
                {
                    float leftEdge = origin.x;
                    float newRightEdge = Mathf.Max(newRight.x, leftEdge + cellSize); // 최소 1칸 유지
                    newRightEdge = Mathf.Round(newRightEdge / cellSize) * cellSize;
                    int newWidth = Mathf.Max(1, Mathf.RoundToInt((newRightEdge - leftEdge) / cellSize));
                    pf.width = newWidth;
                }

                // 아래쪽 변 이동
                if (newBottom != bottom)
                {
                    float topEdge = origin.y + height * cellSize;
                    float newBottomEdge = Mathf.Min(newBottom.y, topEdge - cellSize); // 최소 1칸 유지
                    newBottomEdge = Mathf.Round(newBottomEdge / cellSize) * cellSize;
                    int newHeight = Mathf.Max(1, Mathf.RoundToInt((topEdge - newBottomEdge) / cellSize));
                    pf.origin.y = newBottomEdge;
                    pf.height = newHeight;
                }
                // 위쪽 변 이동
                else if (newTop != top)
                {
                    float bottomEdge = origin.y;
                    float newTopEdge = Mathf.Max(newTop.y, bottomEdge + cellSize); // 최소 1칸 유지
                    newTopEdge = Mathf.Round(newTopEdge / cellSize) * cellSize;
                    int newHeight = Mathf.Max(1, Mathf.RoundToInt((newTopEdge - bottomEdge) / cellSize));
                    pf.height = newHeight;
                }
            }
            EditorUtility.SetDirty(pf);
        }

        // 그리드 시각화 (파란색)
        Handles.color = Color.cyan;
        for (int x = 0; x <= pf.width; x++)
        {
            Vector3 start = pf.origin + new Vector3(x * pf.cellSize, 0, 0);
            Vector3 end = pf.origin + new Vector3(x * pf.cellSize, pf.height * pf.cellSize, 0);
            Handles.DrawLine(start, end);
        }
        for (int y = 0; y <= pf.height; y++)
        {
            Vector3 start = pf.origin + new Vector3(0, y * pf.cellSize, 0);
            Vector3 end = pf.origin + new Vector3(pf.width * pf.cellSize, y * pf.cellSize, 0);
            Handles.DrawLine(start, end);
        }
    }

    // 스냅 함수: axis=0이면 x만, axis=1이면 y만, 생략시 x/y 모두
    private Vector3 SnapPosition(Vector3 pos, float snap, int axis = -1)
    {
        if (axis == 0)
            return new Vector3(Mathf.Round(pos.x / snap) * snap, pos.y, pos.z);
        else if (axis == 1)
            return new Vector3(pos.x, Mathf.Round(pos.y / snap) * snap, pos.z);
        else
            return new Vector3(Mathf.Round(pos.x / snap) * snap, Mathf.Round(pos.y / snap) * snap, pos.z);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlacementGridManager pf = (PlacementGridManager)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Clear All Units"))
        {
            // 유닛 전체 제거 등 디버그 버튼 추가 가능
        }
    }
}
#endif