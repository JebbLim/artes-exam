using UnityEngine;

/// <summary>
/// A generic singleton implementation
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;
    protected static bool applicationIsQuitting = false;
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    // Create a new gameobject instead and add the component
                    GameObject obj = new(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this as T;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    public static bool HasInstance()
    {
        if (applicationIsQuitting)
        {
            return false;
        }
        return _instance != null && _instance.gameObject != null;
    }
}