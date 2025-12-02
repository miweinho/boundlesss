using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    public GameObject promptUI;

    private IInteractable currentInteractable;

    void Update()
    {
        FindInteractable();
        HandleInput();
        UpdatePrompt();
    }

    private void FindInteractable()
    {
        currentInteractable = null;
        Vector2 pos = transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, interactDistance, interactableMask);
        Debug.Log($"Interactor: hits={hits.Length}", this);

        if (hits.Length == 0) return;

        float best = float.MaxValue;
        foreach (var col in hits)
        {
            var interact = col.GetComponents<MonoBehaviour>().OfType<IInteractable>().FirstOrDefault();
            Debug.Log($"  hit {col.name} -> interactable={(interact!=null)}", col);

            if (interact == null) continue;

            float d = (col.transform.position - (Vector3)pos).sqrMagnitude;
            if (d < best)
            {
                best = d;
                currentInteractable = interact;
            }
        }
    }

    private void HandleInput()
    {
        if (currentInteractable == null) return;
        if (Input.GetKeyDown(interactKey) && currentInteractable.CanInteract())
            currentInteractable.Interact();
    }

    private void UpdatePrompt()
    {
        if (promptUI) promptUI.SetActive(currentInteractable != null && currentInteractable.CanInteract());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
