using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualRenderer : MonoBehaviour
{
    public PlacementGridManager gridManager; // 표시에 사용할 그리드 소스

    [Header("Marker")]
    // 이전 single cornerSprite 대신 4개 지원
    [Tooltip("length 4. 0: BL, 1: BR, 2: TL, 3: TR (FourCorners 모드) / 또는 pattern으로 사용")]
    public Sprite[] cornerSprites = new Sprite[4];
    public float markerScale = 1f;
    public bool showCorners = true;

    public enum CornerMode { FourCorners, TiledVariants }
    public CornerMode cornerMode = CornerMode.TiledVariants;

    [Header("Hover")]
    public Sprite hoverSprite;      // 마우스가 올라온 셀 하이라이트
    public float hoverScale = 1f;

    [Header("Rendering")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;

    // 내부
    private GameObject markerParent;
    private List<SpriteRenderer> markers = new List<SpriteRenderer>();
    private GameObject hoverObj;
    private SpriteRenderer hoverRenderer;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        EnsureParents();
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        UpdateHoverIndicator();
    }

    void EnsureParents()
    {
        if (markerParent == null)
        {
            markerParent = transform.Find("GridMarkers")?.gameObject;
            if (markerParent == null)
            {
                markerParent = new GameObject("GridMarkers");
                markerParent.transform.SetParent(transform, false);
            }
        }
    }

    public void Refresh()
    {
        EnsureParents();
        BuildMarkers();
        CreateHoverObject();
    }

    void BuildMarkers()
    {
        // 기존 마커 제거
        foreach (var mr in markers)
            if (mr != null) DestroyImmediate(mr.gameObject);
        markers.Clear();

        if (!showCorners || gridManager == null) return;

        Vector3 worldOrigin = gridManager.transform.TransformPoint(gridManager.origin);

        // 전체 그리드의 네 끝점 좌표 (왼쪽아래, 오른쪽아래, 왼쪽위, 오른쪽위)
        Vector3[] cornerPositions = new Vector3[4];
        cornerPositions[0] = worldOrigin + new Vector3(0 * gridManager.cellSize, 0 * gridManager.cellSize, 0f); // BL
        cornerPositions[1] = worldOrigin + new Vector3(gridManager.width * gridManager.cellSize, 0 * gridManager.cellSize, 0f); // BR
        cornerPositions[2] = worldOrigin + new Vector3(0 * gridManager.cellSize, gridManager.height * gridManager.cellSize, 0f); // TL
        cornerPositions[3] = worldOrigin + new Vector3(gridManager.width * gridManager.cellSize, gridManager.height * gridManager.cellSize, 0f); // TR

        for (int i = 0; i < 4; i++)
        {
            Sprite useSprite = (cornerSprites != null && cornerSprites.Length > i) ? cornerSprites[i] : null;
            if (useSprite == null) continue;

            Vector3 worldPos = cornerPositions[i];
            var go = new GameObject($"CornerMarker_{i}");
            go.transform.SetParent(markerParent.transform, true); // world 좌표 유지
            go.transform.position = worldPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = useSprite;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.transform.localScale = Vector3.one * markerScale;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
            markers.Add(sr);
        }
    }

    void CreateHoverObject()
    {
        if (hoverObj != null) DestroyImmediate(hoverObj);
        if (hoverSprite == null) return;

        hoverObj = new GameObject("GridHover");
        hoverObj.transform.SetParent(transform, true);
        hoverRenderer = hoverObj.AddComponent<SpriteRenderer>();
        hoverRenderer.sprite = hoverSprite;
        hoverRenderer.transform.localScale = Vector3.one * hoverScale;
        hoverRenderer.sortingLayerName = sortingLayerName;
        hoverRenderer.sortingOrder = sortingOrder + 1;
        hoverRenderer.color = new Color(1f, 1f, 1f, 0.9f);
        hoverObj.SetActive(false);
    }

    void UpdateHoverIndicator()
    {
        if (hoverRenderer == null || gridManager == null || hoverSprite == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 world = InputManager.instance.GetPointerWorldPosition();
        world.z = 0f;

        Vector3 worldOrigin = gridManager.transform.TransformPoint(gridManager.origin);

        float localX = world.x - worldOrigin.x;
        float localY = world.y - worldOrigin.y;

        int gx = Mathf.FloorToInt(localX / gridManager.cellSize);
        int gy = Mathf.FloorToInt(localY / gridManager.cellSize);

        if (gx < 0 || gy < 0 || gx >= gridManager.width || gy >= gridManager.height)
        {
            if (hoverObj.activeSelf) hoverObj.SetActive(false);
            return;
        }

        Vector3 cellCenter = worldOrigin + new Vector3((gx + 0.5f) * gridManager.cellSize, (gy + 0.5f) * gridManager.cellSize, 0f);
        hoverObj.transform.position = cellCenter;
        if (!hoverObj.activeSelf) hoverObj.SetActive(true);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureParents();
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                Refresh();
            };
        }
#endif
    }
}
