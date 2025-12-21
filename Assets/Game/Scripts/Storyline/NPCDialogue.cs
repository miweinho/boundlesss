using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Serializable]
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

    [Header("Dialogue Data")]
    public NPCDialogueData dialogueData;

    [Header("UI Prefab (recommended)")]
    [SerializeField] private NPCDialogueView dialogueViewPrefab;
    [SerializeField] private Transform uiParentOverride;
    private NPCDialogueView _viewInstance;

    [Header("UI References (fallback if no prefab)")]
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

    private bool _forceDialogueData;

    public event Action DialogueClosed;

    void Awake()
    {
        EnsureView();
    }

    private void EnsureView()
    {
        if (_viewInstance != null)
            return;

        if (dialogueViewPrefab == null)
            return; // fallback: user wires fields manually

        Transform parent = uiParentOverride != null ? uiParentOverride : null;
        _viewInstance = Instantiate(dialogueViewPrefab, parent);

        // Wire NPCDialogue fields from the view
        dialoguePanel = _viewInstance.dialoguePanel;
        dialogueText = _viewInstance.dialogueText;
        nameText = _viewInstance.npcNameText;
        portraitImage = _viewInstance.portraitImage;

        choicesRoot = _viewInstance.choicesRoot;
        choiceButtonPrefab = _viewInstance.choiceButtonPrefab;

        if (_viewInstance.exitButton != null)
            _viewInstance.exitButton.onClick.AddListener(EndDialogue);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

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

    private void SelectDialogueForCurrentProgress()
    {
        dialogueData = ResolveDialogueData();
    }

    // === IInteractable ===
    public bool CanInteract() => !isDialogueActive;

    public string GetInteractionPrompt() => "Talk";

    public void Interact()
    {
        if (!isDialogueActive) StartDialogue();
        else NextLine();
    }

    /// <summary>
    /// Starts a dialogue with the given data, even if this object is not "in range".
    /// </summary>
    public bool StartDialogueForced(NPCDialogueData data)
    {
        if (isDialogueActive) return false;
        if (data == null) return false;

        dialogueData = data;
        _forceDialogueData = true;

        StartDialogue();
        return true;
    }

    /// <summary>
    /// Starts dialogue using the normal resolution flow (variants/defaultDialogueData),
    /// intended for zone triggers (no interaction prompt needed).
    /// </summary>
    public bool StartDialogueFromTrigger()
    {
        if (isDialogueActive) return false;

        _forceDialogueData = false; // ensure we use SelectDialogueForCurrentProgress()
        StartDialogue();
        return true;
    }

    private void ApplyOptionalUI()
    {
        // Name
        if (nameText != null)
        {
            bool hasName = dialogueData != null && !string.IsNullOrWhiteSpace(dialogueData.npcName);
            nameText.gameObject.SetActive(hasName);
            nameText.SetText(hasName ? dialogueData.npcName : string.Empty);
        }

        // Portrait
        if (portraitImage != null)
        {
            bool hasPortrait = dialogueData != null && dialogueData.npcPortrait != null;
            portraitImage.gameObject.SetActive(hasPortrait);
            portraitImage.sprite = hasPortrait ? dialogueData.npcPortrait : null;
        }
    }

    // === Dialogue Flow ===
    void StartDialogue()
    {
        EnsureView();

        if (!_forceDialogueData)
            SelectDialogueForCurrentProgress();

        _forceDialogueData = false;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCDialogue] No dialogueData resolved on '{name}'.");
            return;
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError($"[NPCDialogue] UI not wired on '{name}'. Need dialoguePanel + dialogueText (or assign dialogueViewPrefab).");
            return;
        }

        isDialogueActive = true;
        _waitingForChoice = false;
        dialogueIndex = 0;

        ApplyOptionalUI();

        dialoguePanel.SetActive(true);
        UpdateInteractIndicator();

        OpenDialogueUI();
        ClearChoices();

        StartCurrentLine();
    }

    void NextLine()
    {
        if (_waitingForChoice)
            return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

            TryShowChoicesForCurrentLine();
            return;
        }

        if (TryShowChoicesForCurrentLine())
            return;

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
            StartCurrentLine();
        else
            EndDialogue();
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

        if (TryShowChoicesForCurrentLine())
            yield break;

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

    private void StartCurrentLine()
    {
        OnEnterLine(dialogueIndex);
        StartCoroutine(TypeLineUnscaled(dialogueData.dialogueLines[dialogueIndex], dialogueData.typingSpeed));
    }

    private void OnChoiceSelected(NPCDialogueData.DialogueChoice choice)
    {
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

        if (choicesRoot != null)
        {
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
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

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nameText != null) nameText.gameObject.SetActive(false);
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
        }

        CloseDialogueUI();
        UpdateInteractIndicator();

        DialogueClosed?.Invoke();
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

    public void ConfigureView(NPCDialogueView viewPrefab, Transform parentOverride = null)
    {
        if (viewPrefab == null) return;

        dialogueViewPrefab = viewPrefab;
        if (parentOverride != null)
            uiParentOverride = parentOverride;

        // Ensure the view exists immediately (so triggers can show it right away)
        EnsureView();
    }
}
