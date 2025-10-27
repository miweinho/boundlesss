using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{
    public int team = 1; // enemies default team 1
    public int maxHP = 10;
    public int currentHP;

    void Awake() => currentHP = maxHP;

    public void ApplyDamage(int amount, Vector2 hitDirection, float knockback, int sourceTeam)
    {
        if (sourceTeam == team) return; // no friendly fire
        currentHP -= amount;
        // TODO: knockback via Rigidbody2D
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        // play death anim, drop loot, etc.
        Destroy(gameObject);
    }
}

