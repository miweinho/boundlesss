using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class ChasingMob2D : BaseMob2D
{
    [Header("Targeting")]
    protected Transform target;
    protected string targetTag = "Player";

    [Header("Chasing Attributes")]
    [SerializeField, Range(0f, 20f)] protected float chaseSpeed = 3f; // default chase speed
    [SerializeField, Range(0f, 10f)] protected float stopDistance = 1f; // default stop distance
    [SerializeField, Range(0f, 50f)] protected float chaseRange = 6f; // start chasing only within this range

    protected override void Awake()
    {
        base.Awake();

        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null)
            {
                target = go.transform;
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Target reference is null in FixedUpdate.");
            return;
        }

        // Calculate direction towards the target
        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        if (dist > chaseRange) return;
        if (dist <= stopDistance) return;

        Vector2 dir = toTarget.normalized;
        rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);

        if (sr != null && dir.x != 0) sr.flipX = dir.x < 0;
    }
}