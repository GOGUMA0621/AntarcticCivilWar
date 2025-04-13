using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardUI : MonoBehaviour
{
   public RectTransform rewardChestViewport;

    private void Start()
    {
        //rewardChestViewport.gameObject.SetActive(false);
    }

    public void OpenViewport()
    {
        rewardChestViewport.gameObject.SetActive(true);
    }
}
