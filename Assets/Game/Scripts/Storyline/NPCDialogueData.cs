using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogueData", menuName = "Dialogue/NPC Dialogue Data")]
public class NPCDialogueData : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [System.Serializable]
    public class DialogueAction
    {
        public string flagKey;
        public bool flagValue = true;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string label;
        [Tooltip("Next dialogueLines index. Use -1 to end.")]
        public int nextLineIndex = -1;

        [Header("Actions (optional)")]
        public DialogueAction[] actions;
    }

    [System.Serializable]
    public class DialogueChoiceSet
    {
        [Tooltip("Index into dialogueLines.")]
        public int lineIndex;
        public DialogueChoice[] choices;

        [Header("Actions when this line is reached (optional)")]
        public DialogueAction[] onEnterLineActions;
    }

    [Header("Choices (optional)")]
    public DialogueChoiceSet[] choiceSets;
}
