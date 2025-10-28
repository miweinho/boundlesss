using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    public Damageable target;
    public Image fill;

    void Start()
    {
        // try to auto-find the fill image if not set in the prefab
        if (fill == null)
            fill = GetComponentInChildren<Image>();

        if (target)
        {
            if (fill == null)
                Debug.LogWarning($"WorldHealthBar on '{gameObject.name}' has no Image assigned/found.", this);

            // Subscribe to changes
            target.OnHealthChanged += UpdateBar;
            // Initialize immediately (guarded inside UpdateBar)
            UpdateBar(target.currentHP, target.maxHP);
        }
    }

    void OnDestroy()
    {
        if (target != null)
            target.OnHealthChanged -= UpdateBar; // prevent memory leak!
    }

    private void UpdateBar(int current, int max)
    {
        if (fill == null)
        {
            // try again in case it wasn't available at Start
            fill = GetComponentInChildren<Image>();
            if (fill == null)
            {
                Debug.LogWarning($"WorldHealthBar.UpdateBar: no Image to update on '{gameObject.name}'", this);
                return;
            }
        }

        if (max <= 0) { fill.fillAmount = 0f; return; }
        fill.fillAmount = (float)current / max;
    }
}
