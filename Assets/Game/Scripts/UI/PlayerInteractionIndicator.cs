using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionIndicator : MonoBehaviour
{
    [Header("Indicator (child or prefab instance)")]
    [SerializeField] private GameObject indicator;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.25f, 0f);

    private readonly HashSet<Collider2D> _nearby = new HashSet<Collider2D>();

    void Awake()
    {
        if (indicator != null)
        {
            indicator.transform.SetParent(transform, worldPositionStays: false);
            indicator.transform.localPosition = localOffset;
            indicator.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsDialogOrInteractable(other)) return;

        _nearby.Add(other);
        SetVisible(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_nearby.Remove(other) && _nearby.Count == 0)
            SetVisible(false);
    }

    private bool IsDialogOrInteractable(Collider2D col)
    {
        // NPC dialogue
        if (col.GetComponentInParent<NPCDialogue>() != null) return true;

        // Storyline/Interactable with dialogue (your class name is Interactable)
        if (col.GetComponentInParent<IInteractable>() != null) return true;

        // Generic interface-based interactables (if you use IInteractable)
        if (col.GetComponentInParent<IInteractable>() != null) return true;

        return false;
    }

    private void SetVisible(bool visible)
    {
        if (indicator != null)
            indicator.SetActive(visible);
    }
}