using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseSync : MonoBehaviour
{
    // Start is called before the first frame update
    private async void Start()
    {
        if (!FirebaseManager.isLoaded)
        {
            await Task.Yield();
        }
    }

}
