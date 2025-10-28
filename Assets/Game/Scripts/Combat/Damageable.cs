using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject healthBarPrefab;

    public int team = 1; // enemies default team 1
    public int maxHP = 10;
    public int currentHP;
    public event Action OnDie;
    public event Action<int, int> OnHealthChanged;

    void Awake() => currentHP = maxHP;

    public void ApplyDamage(int amount, Vector2 hitDirection, float knockback, int sourceTeam)
    {
        if (sourceTeam == team) return; // no friendly fire
        currentHP -= amount;
        OnHealthChanged?.Invoke(currentHP, maxHP);
        // TODO: knockback via Rigidbody2D
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        OnDie?.Invoke();
        // play death anim, drop loot, etc.
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);

    }

    public void ApplyZoneDamage(int amount)
    {
        currentHP -= amount;
        OnHealthChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) Die();
    }

    public void SetMaxHealth(int max, bool setCurrentToMax = true)
    {
        maxHP = max;
        if (setCurrentToMax) currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }


    void Start()
    {
        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab, transform);
            barObj.transform.localPosition = new Vector3(0, 1.0f, 0);

            var bar = barObj.GetComponent<WorldHealthBar>();
            if (bar != null)
                bar.target = this;
            else
                Debug.LogWarning($"healthBarPrefab '{healthBarPrefab.name}' does not contain a WorldHealthBar component.", barObj);
        }
    }
}

