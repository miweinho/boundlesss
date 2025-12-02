using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Dialogue Data")]
    public NPCDialogueData dialogueData;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    [Header("Proximity Indicator")]
    [Tooltip("World-space or screen-space object that shows an '!' when the player is close.")]
    public GameObject interactIndicator;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool playerInRange;

    // === IInteractable ===
    public bool CanInteract()
    {
        // You can add more conditions here if needed.
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    public string GetInteractionPrompt()
{
    // Only show when the NPC is available
    if (!isDialogueActive)
        return "Talk [E]";

    return "";
}

    // === Dialogue Flow ===
    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.name);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);

        // Hide the "!" while we are talking
        UpdateInteractIndicator();

        // TODO: Pause player movement / NPC movement here if you want
        // e.g. GameManager.Instance.SetGamePaused(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        // If we're still typing, instantly finish this line
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        string currentLine = dialogueData.dialogueLines[dialogueIndex];

        foreach (char letter in currentLine)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Auto-progress this line if flagged in the data
        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);

            // Display next line (or end dialogue if this was the last one)
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        // Clear text and hide panel
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

        // TODO: Unpause game if you paused it at StartDialogue
        // GameManager.Instance.SetGamePaused(false);

        // Re-enable the "!" if the player is still in range
        UpdateInteractIndicator();
    }

    // === Proximity "!" Logic ===
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        UpdateInteractIndicator();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        UpdateInteractIndicator();
    }

    private void UpdateInteractIndicator()
    {
        if (interactIndicator == null) return;

        // Show "!" only if:
        // - Player is in range
        // - This NPC can be interacted with (e.g. not already in dialogue)
        bool shouldShow = playerInRange && CanInteract();
        interactIndicator.SetActive(shouldShow);
    }
}
