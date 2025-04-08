using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToImage : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;

    private Sprite lastSprite;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        if (spriteRenderer != null)
        {
            image.sprite = spriteRenderer.sprite;
        }
    }

    void Update()
    {
        if (spriteRenderer != null || image != null)
        {
            if (spriteRenderer.sprite != lastSprite)
            {
                lastSprite = spriteRenderer.sprite;
                image.sprite = lastSprite;
            }
        }
    }
}
