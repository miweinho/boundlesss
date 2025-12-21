using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NPCDialogue))]
public class PortalTeleporter : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string sceneName;

    [Header("Quest Gate (optional)")]
    [SerializeField] private string requiredFlagKey; // e.g. "quest.sylas.completed"
    [SerializeField] private bool requiredFlagValue = true;

    private NPCDialogue _dialogue;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        _dialogue = GetComponent<NPCDialogue>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
            return;

        if (!CanUsePortal())
        {
            // Same setup as NPC: use the NPCDialogue on THIS portal object
            // (defaultDialogueData/variants decide what gets shown)
            _dialogue.StartDialogueFromTrigger();
            return;
        }

        player.MoveAction.Disable();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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

