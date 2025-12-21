using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NPCDialogue))]
public class TutorialDialogueTrigger2D : MonoBehaviour
{
    [Header("One-time Gate")]
    [SerializeField] private string seenFlagKey; // e.g. "tutorial.move.seen"
    [SerializeField] private bool disableTriggerAfterShow = true;

    private Collider2D _col;
    private NPCDialogue _dialogue;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        _dialogue = GetComponent<NPCDialogue>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!string.IsNullOrWhiteSpace(seenFlagKey) && GetFlag(seenFlagKey, false))
            return;

        bool opened = _dialogue.StartDialogueFromTrigger();
        if (!opened)
            return;

        if (!string.IsNullOrWhiteSpace(seenFlagKey))
            SetFlag(seenFlagKey, true);

        if (disableTriggerAfterShow)
            _col.enabled = false;
    }

    private static bool GetFlag(string key, bool defaultValue)
    {
        if (GameStateService.Instance != null) return GameStateService.Instance.GetFlag(key, defaultValue);
        if (GameManager.Instance != null) return GameManager.Instance.GetFlag(key, defaultValue);
        return defaultValue;
    }

    private static void SetFlag(string key, bool value)
    {
        if (GameStateService.Instance != null) GameStateService.Instance.SetFlag(key, value);
        else if (GameManager.Instance != null) GameManager.Instance.SetFlag(key, value);
    }
}