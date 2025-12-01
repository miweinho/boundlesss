using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseMob2D : MonoBehaviour
{
    [Header("Base Mob Attributes")]
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    protected virtual void Start()
    {
    }

    protected virtual void FixedUpdate()
    {
        // Base mob does not implement movement
    }
}