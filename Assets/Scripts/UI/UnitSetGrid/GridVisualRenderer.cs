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

    [Header("Behavior")]
    [Tooltip("마커가 이미 존재하면 새로 생성하지 않음(기본 true). 강제 재생성하려면 Refresh(true) 호출)")]
    public bool preventDuplicateMarkers = true;

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

        // 기존에 씬에 GridHover가 이미 있으면 재사용하도록 캐시
        var existing = transform.Find("GridHover");
        if (existing != null)
        {
            hoverObj = existing.gameObject;
            hoverRenderer = hoverObj.GetComponent<SpriteRenderer>();
            if (hoverRenderer == null) hoverRenderer = hoverObj.AddComponent<SpriteRenderer>();
        }
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

    public void Refresh(bool forceRebuild = false)
    {
        EnsureParents();

        if (forceRebuild)
        {
            // 안전하게 삭제: editor에서는 DestroyImmediate, 런타임에서는 Destroy
#if UNITY_EDITOR
            foreach (var mr in markers)
                if (mr != null) DestroyImmediate(mr.gameObject);
#else
            foreach (var mr in markers)
                if (mr != null) Destroy(mr.gameObject);
#endif
            markers.Clear();

            // markerParent 자식 제거
            for (int i = markerParent.transform.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                DestroyImmediate(markerParent.transform.GetChild(i).gameObject);
#else
                Destroy(markerParent.transform.GetChild(i).gameObject);
#endif
            }

            BuildMarkers();
        }
        else
        {
            BuildMarkers();
        }

        // hover는 중복 생성 방지 로직 내에서 처리
        CreateHoverObject();
    }

    void BuildMarkers()
    {
        // 이미 마커가 존재하고 중복 방지가 켜져 있으면 기존 마커 목록만 캐시하고 종료
        if (preventDuplicateMarkers && markerParent != null && markerParent.transform.childCount > 0)
        {
            markers.Clear();
            foreach (Transform child in markerParent.transform)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) markers.Add(sr);
            }
            return;
        }

        // 기존 마커 제거
        foreach (var mr in markers)
            if (mr != null) DestroyImmediate(mr.gameObject);
        markers.Clear();

        // markerParent의 기존 자식들도 제거
        if (markerParent != null)
        {
            for (int i = markerParent.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(markerParent.transform.GetChild(i).gameObject);
        }

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
        // 이미 하이라이트 객체가 있으면 재사용 및 설정 업데이트
        if (hoverObj == null)
        {
            // 같은 이름의 기존 오브젝트가 있나 검색(다른 스크립트가 만든 경우 포함)
            var existing = transform.Find("GridHover");
            if (existing != null)
            {
                hoverObj = existing.gameObject;
                hoverRenderer = hoverObj.GetComponent<SpriteRenderer>();
                if (hoverRenderer == null) hoverRenderer = hoverObj.AddComponent<SpriteRenderer>();
            }
            else
            {
                if (hoverSprite == null) return;
                hoverObj = new GameObject("GridHover");
                hoverObj.transform.SetParent(transform, false);
                hoverRenderer = hoverObj.AddComponent<SpriteRenderer>();
            }
        }

        // 중복 GridHover 정리: 같은 이름의 다른 오브젝트가 있다면 제거
        int found = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "GridHover")
            {
                if (found == 0)
                {
                    // 첫번째는 유지
                    found++;
                    continue;
                }
                // 나머지 중복은 제거
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        // renderer 설정 갱신
        if (hoverRenderer != null)
        {
            hoverRenderer.sprite = hoverSprite;
            hoverRenderer.transform.localScale = Vector3.one * hoverScale;
            hoverRenderer.sortingLayerName = sortingLayerName;
            hoverRenderer.sortingOrder = sortingOrder + 1;
            hoverRenderer.color = new Color(1f, 1f, 1f, 0.9f);
            if (!hoverObj.activeSelf) hoverObj.SetActive(false);
        }
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
