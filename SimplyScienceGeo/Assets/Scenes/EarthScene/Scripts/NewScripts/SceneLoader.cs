using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // Optional: scene that shows if no URL arg supplied
    [SerializeField] private string fallbackScene = "MapScene";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // survive scene switches
            LoadFromArgsOrFallback();
        }
        else
            Destroy(gameObject);
    }

    /* ––––– public API (called from JS or UI buttons) ––––– */
    public void LoadScene(string sceneName)
    {
        // If you prefer indexes use: SceneManager.LoadSceneAsync(index);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    /* ––––– helpers ––––– */
    void LoadFromArgsOrFallback()
    {
        string arg = GetCmdArg("startScene");
        if (!string.IsNullOrEmpty(arg))
            LoadScene(arg);
        else
            LoadScene(fallbackScene);
    }

    static string GetCmdArg(string key)
    {
        string prefix = $"--{key}=";
        foreach (string a in System.Environment.GetCommandLineArgs())
            if (a.StartsWith(prefix)) return a.Substring(prefix.Length);
        return null;
    }
}
