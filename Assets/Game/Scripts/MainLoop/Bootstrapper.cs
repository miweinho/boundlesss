using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        // Ensure the global game-state service exists (flags/quests/decisions).
        if (GameStateService.Instance == null)
        {
            var go = new GameObject("GameStateService");
            go.AddComponent<GameStateService>();
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ShowMainMenuOverlay();
        else
            Debug.LogError("[Bootstrapper] GameManager.Instance is null. Ensure GameManager exists in Bootstrap scene.");
    }
}