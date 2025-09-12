using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (IsRoot(_instance.gameObject))
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private bool IsRoot(GameObject gameObject)
    {
        return gameObject.transform.root == gameObject.transform;
    }
}
