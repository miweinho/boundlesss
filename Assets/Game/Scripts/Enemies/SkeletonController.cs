using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WeaponHolder))]
public class SkeletonController : ChasingMob2D
{
    [Header("Ranged Attack")]
    [SerializeField, Range(0f, 20f)]
    private float attackRange = 6f;

    private WeaponHolder weaponHolder;

    protected override void Awake()
    {
        base.Awake();
        weaponHolder = GetComponent<WeaponHolder>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Update()
    {
        if (target == null || weaponHolder == null)
            return;

        // Work out the aim direction each round 
        Vector2 handPos = weaponHolder.HandTransform.position;
        Vector2 toTarget = (Vector2)target.position - handPos;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Vector2 aimDir = toTarget.normalized;
            weaponHolder.SetAim(aimDir);

            // Optional: flip the sprite based on aim
            if (sr != null && Mathf.Abs(aimDir.x) > 0.01f)
                sr.flipX = aimDir.x < 0f;
        }

        // Attack if within the attack range
        float dist = Vector2.Distance(target.position, transform.position);
        if (dist <= attackRange)
        {
            weaponHolder.TryAttack();
        }
    }
}


