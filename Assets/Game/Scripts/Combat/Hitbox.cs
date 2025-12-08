using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    [SerializeField] protected float lifetime = 0.1f;
    [SerializeField] protected Vector2 size = new Vector2(1f, 0.6f); // can be tuned per prefab

    // Made protected so subclasses can see these if needed
    protected int damage;
    protected float knockback;
    protected int sourceTeam;
    protected Vector2 dir;

    protected virtual void Awake()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.isTrigger = true;
            box.size = size;
        }
        else
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }
    }

    public virtual void Configure(
        int damage,
        float knockback,
        int sourceTeam,
        Vector2 dir,
        Collider2D ignoreCollider = null)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.sourceTeam = sourceTeam;
        this.dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;

        var col = GetComponent<Collider2D>();
        if (ignoreCollider != null && col != null)
            Physics2D.IgnoreCollision(col, ignoreCollider, true);

        Destroy(gameObject, lifetime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.ApplyDamage(damage, dir, knockback, sourceTeam);
        }
    }
}
