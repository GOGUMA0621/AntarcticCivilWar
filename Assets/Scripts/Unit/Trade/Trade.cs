using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Trade : MonoBehaviour
{
    public GameObject tradeUI;
    public TextMeshPro textTrader;
    bool isTrade = false;

    PlayerController playerController;
    private void Start()
    {
        textTrader.enabled = false;
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(this.transform.position, playerController.playerPos);

        if (distanceToPlayer <= 5)
        {
            ClosePlayer();
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!isTrade)
                {
                    OpenShop();
                }
                else
                {
                    CloseShop();
                }
            }
        }
        else
        {
            FarPlayer();
        }
    }

    private void ClosePlayer()
    {
        textTrader.enabled = true;
    }

    private void FarPlayer()
    {
        textTrader.enabled = false;
    }

    private void CloseShop()
    {
        tradeUI.gameObject.SetActive(false);
        Time.timeScale = 1;
        isTrade = false;
    }

    private void OpenShop()
    {
        tradeUI.gameObject.SetActive(true);
        Time.timeScale = 0;
        isTrade = true;
    }
}
