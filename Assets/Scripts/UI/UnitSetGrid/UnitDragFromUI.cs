using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDragFromUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject unitPrefab { get; set; }
    private GameObject previewUnit;
    private Canvas canvas;
    private PlacementGridManager grid;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        grid = FindObjectOfType<PlacementGridManager>();
    }

    /// <summary>
    /// 유닛 슬롯에서 드래그를 시작할 때 호출
    /// 프리뷰 유닛을 생성하고 충돌 및 물리 효과를 비활성화
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {

        if (unitPrefab == null)
        {
            Debug.LogWarning("unitPrefab이 설정되지 않음");
            return;
        }

        previewUnit = Instantiate(unitPrefab);
        previewUnit.name = "[Preview] " + unitPrefab.name;

        var col = previewUnit.GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = previewUnit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// 드래그 중 마우스 위치에 따라 프리뷰 유닛의 위치를 업데이트.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (previewUnit == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        previewUnit.transform.position = worldPos;
    }

    /// <summary>
    /// 드래그 종료 시 유닛을 배치 가능한 위치에 생성하고, 프리뷰 유닛을 제거
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (previewUnit == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        Vector2Int gridPos = GridUtility.WorldToGrid(worldPos, grid.origin, grid.cellSize);

        if (grid.CanPlace(gridPos))
        {
            grid.PlaceUnit(previewUnit.GetComponent<Unit>(), gridPos);
        }

        var col = previewUnit.GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = true;

        var rb = previewUnit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.isKinematic = false;
        }
    }
}
