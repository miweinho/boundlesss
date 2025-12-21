using UnityEngine;
using UnityEngine.SceneManagement;

public static class AutoBootstrap
{
    private const string AppRootResourcePath = "AppRoot";

#if UNITY_EDITOR
    // not const => no unreachable-code warning
    private static bool ResetStateOnEveryPlayInEditor = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureAppRoot()
    {
#if UNITY_2023_1_OR_NEWER
        if (Object.FindFirstObjectByType<GameManager>() != null)
            return;
#else
        if (Object.FindObjectOfType<GameManager>() != null)
            return;
#endif

        var prefab = Resources.Load<GameObject>(AppRootResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"[AutoBootstrap] Missing Resources/{AppRootResourcePath}.prefab");
            return;
        }

        Object.Instantiate(prefab);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetAndForcePlaying()
    {
        if (!ResetStateOnEveryPlayInEditor)
            return;

        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Bootstrap" || sceneName == "MainMenu")
            return;

        // Reset flags (if you want a clean debug state every time)
        if (GameStateService.Instance != null)
        {
            GameStateService.Instance.DeleteSave();
            GameStateService.Instance.Load();
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ForceGameplayForTesting();
    }
#endif
}
