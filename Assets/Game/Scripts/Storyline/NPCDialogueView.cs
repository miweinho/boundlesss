using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogueView : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject dialoguePanel;

    [Header("UI")]
    public TMP_Text npcNameText;
    public Image portraitImage;
    public TMP_Text dialogueText;

    [Header("Choices")]
    public Transform choicesRoot;
    public Button choiceButtonPrefab;

    [Header("Controls")]
    public Button exitButton;
}