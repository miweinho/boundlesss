using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Collider2D selfCollider;
    private int damage;
    private float knockback;
    private int sourceTeam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        selfCollider = col;
        
    } 

    void FixedUpdate()
    {
        // Point sprite in movement direction
        if (rb.velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle; // rotates the Rigidbody (and SpriteRenderer)
        }
    }

    public void Fire(Vector2 dir, int damage, float knockback, int sourceTeam, float speedScalar, Collider2D ignoreCollider = null)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.sourceTeam = sourceTeam;
        rb.velocity = dir.normalized * speed * Mathf.Max(0.01f, speedScalar);
        if (ignoreCollider != null && selfCollider != null)
        {
            Physics2D.IgnoreCollision(selfCollider, ignoreCollider, true);
        }
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
