using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenSlot : MonoBehaviour
{
    public static InvenSlot Instance;

    [SerializeField] private Image rarity_Frame;
    [SerializeField] private Image coolTime_Img;
    [SerializeField] private Image icon_Img;

    private float maxCoolTime = 0f;
    private float currentCoolTime = 0f;
    private bool isCooling = false;
    private ItemDB currentItem;

    private void Update()
    {
        if (isCooling)
        {
            currentCoolTime += Time.deltaTime;
            coolTime_Img.fillAmount = currentCoolTime / maxCoolTime;

            if (currentCoolTime >= maxCoolTime)
            {
                isCooling = false;
                coolTime_Img.fillAmount = 0f;
            }
        }
    }

    public void SetItemImg(ItemDB item)
    {
        currentItem = item;

        //rarity_Frame.sprite = Resources.Load<Sprite>($"ItemFrame/{item.rarity}");
        coolTime_Img.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
        icon_Img.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
    }

    public void StartCoolTime()
    {
        if (currentItem.cooltime > 0)
        {
            maxCoolTime = currentItem.cooltime;
            currentCoolTime = 0f;
            isCooling = true;
            coolTime_Img.fillAmount = 0f;
        }
        else
            maxCoolTime = 0f;
    }
}
