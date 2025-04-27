using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameExpandFollower : MonoBehaviour
{
    [SerializeField] private RectTransform frame;
    [SerializeField] private RectTransform frameExpand;
    [SerializeField] private float offsetX = -200f;

    private float initialY;

    private void Start()
    {
        initialY = frameExpand.anchoredPosition.y;
    }

    private void Update()
    {
        float frameWidth = frame.rect.width;
        frameExpand.anchoredPosition = new Vector2(frameWidth + offsetX, initialY);
    }
}
