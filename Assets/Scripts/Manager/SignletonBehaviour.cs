using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
            return;
        }
        _instance = GetComponent<T>();
        if (IsRoot(_instance.gameObject))
        {
            DontDestroyOnLoad(_instance.gameObject);
        }
    }

    private bool IsRoot(GameObject gameObject)
    {
        return gameObject.transform.root == gameObject.transform;
    }
}
