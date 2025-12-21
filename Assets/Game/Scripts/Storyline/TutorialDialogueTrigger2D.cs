using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TutorialDialogueTrigger2D : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialoguePresenter;
    [SerializeField] private NPCDialogueData dialogueData;

    [Header("One-shot (optional)")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string seenFlagKey; // e.g. "tutorial.move.seen"

    [Header("Player Control")]
    [SerializeField] private bool disablePlayerMoveWhileOpen = true;

    private bool _triggered;
    private PlayerController _player;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrWhiteSpace(seenFlagKey))
        {
            bool seen =
                GameStateService.Instance != null ? GameStateService.Instance.GetFlag(seenFlagKey, false) :
                GameManager.Instance != null ? GameManager.Instance.GetFlag(seenFlagKey, false) :
                false;

            if (seen) return;
        }

        if (dialoguePresenter == null || dialogueData == null) return;

        _player = other.GetComponent<PlayerController>();

        if (disablePlayerMoveWhileOpen && _player != null)
            _player.MoveAction.Disable();

        dialoguePresenter.DialogueClosed -= OnDialogueClosed;
        dialoguePresenter.DialogueClosed += OnDialogueClosed;

        if (dialoguePresenter.StartDialogueForced(dialogueData))
        {
            _triggered = true;

            if (!string.IsNullOrWhiteSpace(seenFlagKey))
            {
                if (GameStateService.Instance != null) GameStateService.Instance.SetFlag(seenFlagKey, true);
                else if (GameManager.Instance != null) GameManager.Instance.SetFlag(seenFlagKey, true);
            }

            if (triggerOnce)
                GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            // If it didn't open, restore movement
            if (disablePlayerMoveWhileOpen && _player != null)
                _player.MoveAction.Enable();
        }
    }

    private void OnDialogueClosed()
    {
        if (dialoguePresenter != null)
            dialoguePresenter.DialogueClosed -= OnDialogueClosed;

        if (disablePlayerMoveWhileOpen && _player != null)
            _player.MoveAction.Enable();

        _player = null;
    }
}