using UnityEngine;

/// <summary>
/// Ensures this GameObject is not destroyed when loading a new scene.
/// Also enforces that only one instance of this GameObject ever exists.
/// </summary>
public class DontDestroy : MonoBehaviour
{
    public static DontDestroy instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}