using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseLoader : MonoBehaviour
{
    public static bool IsLoaded { get; private set; }

    private async void Start()
    {
        await FirebaseManager.ItemLoadData();
        await FirebaseManager.UnitLoadData();

        IsLoaded = true;
        Debug.Log("Firebase 데이터 로딩 완료");
    }
}
