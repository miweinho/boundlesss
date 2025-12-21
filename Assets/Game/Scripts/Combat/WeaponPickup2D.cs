using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeaponPickup2D : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private WeaponData weapon;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    [SerializeField] private bool equipOnPickup = true;
    [SerializeField] private bool equipOnlyIfEmpty = true;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Optional Indicator")]
    [SerializeField] private GameObject interactIndicator;

    private bool _picked;
    private WeaponInventory _cachedInventory; // <- fix: existiert jetzt

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        SetIndicator(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_picked) return;
        if (!other.CompareTag("Player")) return;

        _cachedInventory = other.GetComponentInParent<WeaponInventory>();
        SetIndicator(_cachedInventory != null);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _cachedInventory = null;
        SetIndicator(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_picked) return;
        if (_cachedInventory == null) return;
        if (!other.CompareTag("Player")) return;

        if (Input.GetKeyDown(pickupKey))
            Pickup();
    }

    private void Pickup()
    {
        if (weapon == null)
        {
            Debug.LogWarning("[WeaponPickup2D] No WeaponData assigned.", this);
            return;
        }

        if (_cachedInventory == null)
        {
            Debug.LogWarning("[WeaponPickup2D] No WeaponInventory cached on player.", this);
            return;
        }

        _picked = true;
        SetIndicator(false);

        var sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        _cachedInventory.AddWeapon(weapon, equipOnPickup, equipOnlyIfEmpty);

        if (destroyOnPickup) Destroy(gameObject, 0.1f);
        else gameObject.SetActive(false);
    }

    private void SetIndicator(bool visible)
    {
        if (interactIndicator != null)
            interactIndicator.SetActive(visible);
    }
}