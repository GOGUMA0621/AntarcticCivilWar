using UnityEngine;

public class SpriteLayout : MonoBehaviour
{
    public enum LayoutType { Horizontal, Vertical }
    public enum PivotType { Left, Center, Right }

    public LayoutType layoutType = LayoutType.Horizontal;
    public PivotType pivot = PivotType.Center;
    public float spacing = 1.0f; // 오브젝트 간 간격

    void Start()
    {
        Arrange();
    }

    public void Arrange()
    {
        int count = transform.childCount;
        float totalLength = (count - 1) * spacing;
        float offset = 0f;

        switch (pivot)
        {
            case PivotType.Left:
                offset = 0f;
                break;
            case PivotType.Center:
                offset = -totalLength / 2f;
                break;
            case PivotType.Right:
                offset = -totalLength;
                break;
        }

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 pos = Vector3.zero;
            if (layoutType == LayoutType.Horizontal)
                pos = new Vector3(offset + i * spacing, 0, 0);
            else
                pos = new Vector3(0, offset - i * spacing, 0);
            child.localPosition = pos;
        }
    }

#if UNITY_EDITOR
    // 에디터에서 실시간 확인용
    void OnValidate()
    {
        Arrange();
    }
#endif
}
