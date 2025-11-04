using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    public Damageable target;
    public Slider healthBar;

    void Start()
    {

        healthBar = GetComponent<Slider>();
        healthBar.maxValue = target.maxHP;
        healthBar.value = target.currentHP;
            

        if (target)
        {
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
        if (healthBar == null)
        {
            // try again in case it wasn't available at Start
            healthBar = GetComponentInChildren<Slider>();
            if (healthBar == null)
            {
                Debug.LogWarning($"WorldHealthBar.UpdateBar: no Image to update on '{gameObject.name}'", this);
                return;
            }
        }

        healthBar.maxValue = max;
        healthBar.value = current;
    }
}
