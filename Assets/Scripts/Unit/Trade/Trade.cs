using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trade : MonoBehaviour
{
    public GameObject tradeUI;
    bool isTrade = false;

    PlayerController playerController;
    private void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(this.transform.position, playerController.playerPos);
        
        if(distanceToPlayer <= 5)
        {
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
