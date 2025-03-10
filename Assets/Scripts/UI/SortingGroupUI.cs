using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SortingGroupUI : MonoBehaviour
{
    int childCount;
    RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        childCount = transform.childCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (childCount >= 2)
        {
            rect.localPosition = new Vector3((childCount - 1) / 2, rect.localPosition.y, rect.localPosition.z);
        }
    }
}
