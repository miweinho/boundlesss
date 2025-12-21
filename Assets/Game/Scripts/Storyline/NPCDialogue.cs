using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class DialogueVariant
    {
        public string id;
        public NPCDialogueData data;

        [Header("Conditions (optional)")]
        public string requiredFlagKey;
        public bool requiredFlagValue = true;

        [Tooltip("Higher priority wins.")]
        public int priority = 0;
    }

    [Header("Dialogue Variants")]
    [SerializeField] private List<DialogueVariant> variants = new();
    [SerializeField] private NPCDialogueData defaultDialogueData;

    private DialogueVariant _activeVariant;

    private NPCDialogueData ResolveDialogueData()
    {
        DialogueVariant best = null;

        for (int i = 0; i < variants.Count; i++)
        {
            var v = variants[i];
            if (v == null || v.data == null) continue;

            if (!string.IsNullOrWhiteSpace(v.requiredFlagKey))
            {
                bool flag = GetFlag(v.requiredFlagKey, false);
                if (flag != v.requiredFlagValue)
                    continue;
            }

            if (best == null || v.priority > best.priority)
                best = v;
        }

        _activeVariant = best;
        return best != null ? best.data : defaultDialogueData;
    }

    private bool GetFlag(string key, bool defaultValue)
    {
        if (GameStateService.Instance != null) return GameStateService.Instance.GetFlag(key, defaultValue);
        if (GameManager.Instance != null) return GameManager.Instance.GetFlag(key, defaultValue);
        return defaultValue;
    }

    // Call this at the start of your dialogue opening logic:
    private void SelectDialogueForCurrentProgress()
    {
        dialogueData = ResolveDialogueData();
    }

    [Header("Dialogue Data")]
    public NPCDialogueData dialogueData;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    [Header("Choices UI")]
    [SerializeField] private Transform choicesRoot;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Proximity Indicator")]
    public GameObject interactIndicator;

    private readonly List<Button> _spawnedChoiceButtons = new();

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool playerInRange;
    private bool _didRequestDialogPause;
    private bool _waitingForChoice;

    void Awake()
    {
        SetChoicesVisible(false);
    }

    // === IInteractable ===
    public bool CanInteract() => !isDialogueActive;

    public void Interact()
    {
        // Don't early-return here: dialogueData may be selected inside StartDialogue().
        if (!isDialogueActive) StartDialogue();
        else NextLine();
    }

    public string GetInteractionPrompt() => !isDialogueActive ? "Talk [E]" : "";

    // === Dialogue Flow ===
    void StartDialogue()
    {
        SelectDialogueForCurrentProgress();

        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCDialogue] No dialogue data resolved on '{name}'. Assign Default Dialogue Data or add a matching Variant.");
            return;
        }

        isDialogueActive = true;
        _waitingForChoice = false;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.name);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        UpdateInteractIndicator();

        OpenDialogueUI();
        ClearChoices();

        StartCurrentLine();
    }

    void NextLine()
    {
        if (_waitingForChoice)
            return; // Choices must be answered first

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

            // If this line has choices, show them immediately after fast-forward.
            TryShowChoicesForCurrentLine();
            return;
        }

        // If this line has choices, do not advance automatically.
        if (TryShowChoicesForCurrentLine())
            return;

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLineUnscaled(string line, float charsPerSecond)
    {
        isTyping = true;
        _waitingForChoice = false;
        ClearChoices();

        dialogueText.SetText("");

        charsPerSecond = Mathf.Max(1f, charsPerSecond);
        float secondsPerChar = 1f / charsPerSecond;

        int index = 0;
        float acc = 0f;

        while (index < line.Length)
        {
            acc += Time.unscaledDeltaTime;

            while (acc >= secondsPerChar && index < line.Length)
            {
                acc -= secondsPerChar;
                dialogueText.text += line[index];
                index++;
            }

            yield return null;
        }

        isTyping = false;

        // If there are choices, present them; do not auto-progress.
        if (TryShowChoicesForCurrentLine())
            yield break;

        // Auto-progress only when enabled for this line AND there are no choices.
        if (dialogueData.autoProgressLines != null &&
            dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return WaitSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private bool TryShowChoicesForCurrentLine()
    {
        var choices = GetChoicesForLine(dialogueIndex);
        if (choices == null || choices.Length == 0)
            return false;

        ShowChoices(choices);
        _waitingForChoice = true;
        return true;
    }

    private NPCDialogueData.DialogueChoice[] GetChoicesForLine(int lineIndex)
    {
        if (dialogueData == null || dialogueData.choiceSets == null) return null;

        for (int i = 0; i < dialogueData.choiceSets.Length; i++)
        {
            var set = dialogueData.choiceSets[i];
            if (set != null && set.lineIndex == lineIndex)
                return set.choices;
        }

        return null;
    }

    private void ShowChoices(NPCDialogueData.DialogueChoice[] choices)
    {
        ClearChoices();

        if (choicesRoot == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("[NPCDialogue] Choices UI not wired (choicesRoot/choiceButtonPrefab).");
            return;
        }

        SetChoicesVisible(true);

        for (int i = 0; i < choices.Length; i++)
        {
            var choice = choices[i];
            if (choice == null) continue;

            var btn = Instantiate(choiceButtonPrefab, choicesRoot);
            _spawnedChoiceButtons.Add(btn);

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = choice.label;

            btn.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    private void ApplyActions(NPCDialogueData.DialogueAction[] actions)
    {
        if (actions == null || actions.Length == 0) return;

        for (int i = 0; i < actions.Length; i++)
        {
            var a = actions[i];
            if (a == null || string.IsNullOrWhiteSpace(a.flagKey)) continue;

            // Prefer GameStateService (Option B). Fallback keeps project working if service isn't present.
            if (GameStateService.Instance != null)
                GameStateService.Instance.SetFlag(a.flagKey, a.flagValue);
            else if (GameManager.Instance != null)
                GameManager.Instance.SetFlag(a.flagKey, a.flagValue);
        }
    }

    private NPCDialogueData.DialogueChoiceSet GetChoiceSetForLine(int lineIndex)
    {
        if (dialogueData == null || dialogueData.choiceSets == null) return null;

        for (int i = 0; i < dialogueData.choiceSets.Length; i++)
        {
            var set = dialogueData.choiceSets[i];
            if (set != null && set.lineIndex == lineIndex)
                return set;
        }

        return null;
    }

    private void OnEnterLine(int lineIndex)
    {
        var set = GetChoiceSetForLine(lineIndex);
        if (set != null)
            ApplyActions(set.onEnterLineActions);
    }

    // Call this right before you start typing a line (or right after setting dialogueIndex).
    private void StartCurrentLine()
    {
        OnEnterLine(dialogueIndex);
        StartCoroutine(TypeLineUnscaled(dialogueData.dialogueLines[dialogueIndex], dialogueData.typingSpeed));
    }

    private void OnChoiceSelected(NPCDialogueData.DialogueChoice choice)
    {
        // Apply choice actions (flags) immediately on click
        ApplyActions(choice.actions);

        _waitingForChoice = false;
        ClearChoices();

        if (choice == null || choice.nextLineIndex < 0)
        {
            EndDialogue();
            return;
        }

        dialogueIndex = Mathf.Clamp(choice.nextLineIndex, 0, dialogueData.dialogueLines.Length - 1);
        StartCoroutine(TypeLineUnscaled(dialogueData.dialogueLines[dialogueIndex], dialogueData.typingSpeed));
    }

    private void ClearChoices()
    {
        for (int i = 0; i < _spawnedChoiceButtons.Count; i++)
        {
            if (_spawnedChoiceButtons[i] != null)
                Destroy(_spawnedChoiceButtons[i].gameObject);
        }
        _spawnedChoiceButtons.Clear();

        // Also clear any template/leftover children under the root (e.g., a button placed in the scene).
        if (choicesRoot != null)
        {
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(choicesRoot.GetChild(i).gameObject);
            }
        }

        SetChoicesVisible(false);
    }

    private void SetChoicesVisible(bool visible)
    {
        if (choicesRoot != null && choicesRoot.gameObject.activeSelf != visible)
            choicesRoot.gameObject.SetActive(visible);
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        isDialogueActive = false;
        _waitingForChoice = false;

        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

        CloseDialogueUI();
        UpdateInteractIndicator();
    }

    private void OpenDialogueUI()
    {
        if (!_didRequestDialogPause && GameManager.Instance != null)
        {
            GameManager.Instance.BeginDialogPause();
            _didRequestDialogPause = true;
        }
    }

    private void CloseDialogueUI()
    {
        if (_didRequestDialogPause && GameManager.Instance != null)
        {
            GameManager.Instance.EndDialogPause();
            _didRequestDialogPause = false;
        }
    }

    private void OnDisable()
    {
        if (_didRequestDialogPause && GameManager.Instance != null)
        {
            GameManager.Instance.EndDialogPause();
            _didRequestDialogPause = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        UpdateInteractIndicator();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        UpdateInteractIndicator();
    }

    private void UpdateInteractIndicator()
    {
        if (interactIndicator == null) return;
        interactIndicator.SetActive(playerInRange && CanInteract());
    }
}
