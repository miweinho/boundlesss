using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.1f;
    [SerializeField] private Vector2 size = new Vector2(1f, 0.6f); // can be tuned per prefab

    private int damage;
    private float knockback;
    private int sourceTeam;
    private Vector2 dir;

    public void Configure(int damage, float knockback, int sourceTeam, Vector2 dir)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.sourceTeam = sourceTeam;
        this.dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
            dmg.ApplyDamage(damage, dir, knockback, sourceTeam);
    }
}
