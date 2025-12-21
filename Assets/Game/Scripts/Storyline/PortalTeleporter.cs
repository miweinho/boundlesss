using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortalTeleporter : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string sceneName;

    [Header("Quest Gate (optional)")]
    [SerializeField] private string requiredFlagKey; // e.g. "quest.sylas.completed"
    [SerializeField] private bool requiredFlagValue = true;

    [Header("Locked Dialogue")]
    [Tooltip("Scene object with NPCDialogue (Presenter). If null, prefab will be instantiated.")]
    [SerializeField] private NPCDialogue dialoguePresenter;

    [Tooltip("Optional prefab that contains NPCDialogue + your dialogue UI wired.")]
    [SerializeField] private NPCDialogue dialoguePresenterPrefab;

    [SerializeField] private NPCDialogueData lockedDialogueData;

    private NPCDialogue _spawnedPresenter;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
            return;

        if (!CanUsePortal())
        {
            var presenter = GetPresenter();
            if (presenter != null && lockedDialogueData != null)
                presenter.StartDialogueForced(lockedDialogueData);

            return;
        }

        // Teleport
        player.MoveAction.Disable();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private NPCDialogue GetPresenter()
    {
        if (dialoguePresenter != null)
            return dialoguePresenter;

        if (_spawnedPresenter != null)
            return _spawnedPresenter;

        if (dialoguePresenterPrefab == null)
        {
            Debug.LogWarning("[PortalTeleporter] No dialogue presenter assigned (scene ref or prefab).");
            return null;
        }

        _spawnedPresenter = Instantiate(dialoguePresenterPrefab);
        return _spawnedPresenter;
    }

    private bool CanUsePortal()
    {
        if (string.IsNullOrWhiteSpace(requiredFlagKey))
            return true;

        bool current =
            GameStateService.Instance != null ? GameStateService.Instance.GetFlag(requiredFlagKey, false) :
            GameManager.Instance != null ? GameManager.Instance.GetFlag(requiredFlagKey, false) :
            false;

        return current == requiredFlagValue;
    }
}

