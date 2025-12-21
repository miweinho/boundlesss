using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private bool showMainMenuOnStart = true;

    void Start()
    {
        if (!showMainMenuOnStart) return;

        if (GameManager.Instance != null)
            GameManager.Instance.ShowMainMenuOverlay();
        else
            Debug.LogError("[Bootstrapper] GameManager.Instance is null. Ensure GameManager exists in Bootstrap scene.");
    }
}