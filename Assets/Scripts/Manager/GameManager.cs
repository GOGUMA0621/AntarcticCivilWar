using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        while (!FirebaseLoader.IsLoaded)
            await Task.Yield();
    }
}
