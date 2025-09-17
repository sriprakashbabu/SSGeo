using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private string fallbackScene = "MapScene";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // ❌ Don't load here
            // LoadFromArgsOrFallback();
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // ✅ Load at end-of-frame
        string target = GetCmdArg("startScene");
        if (string.IsNullOrEmpty(target)) target = fallbackScene;
        StartCoroutine(LoadNextFrame(target));
    }

    IEnumerator LoadNextFrame(string sceneName)
    {
        yield return null; // let bootstrapper finish initial frame
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        // Safety: ensure one EventSystem exists (helps if something got stripped/missing)
        if (EventSystem.current == null)
        {
            var go = new GameObject("EventSystem");
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        // Optional: hand off and remove bootstrapper
        Destroy(gameObject);
    }

    public void LoadScene(string sceneName) =>
        StartCoroutine(LoadNextFrame(sceneName));

    static string GetCmdArg(string key)
    {
        string prefix = $"--{key}=";
        foreach (string a in System.Environment.GetCommandLineArgs())
            if (a.StartsWith(prefix)) return a.Substring(prefix.Length);
        return null;
    }
}
