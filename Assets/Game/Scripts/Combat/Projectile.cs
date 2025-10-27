using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private int damage;
    private float knockback;
    private int sourceTeam;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Fire(Vector2 dir, int damage, float knockback, int sourceTeam, float speedScalar)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.sourceTeam = sourceTeam;
        rb.velocity = dir.normalized * speed * Mathf.Max(0.01f, speedScalar);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            var dir = rb.velocity.sqrMagnitude > 0.001f ? rb.velocity.normalized : Vector2.right;
            dmg.ApplyDamage(damage, dir, knockback, sourceTeam);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject); // hit wall
        }
    }
}
