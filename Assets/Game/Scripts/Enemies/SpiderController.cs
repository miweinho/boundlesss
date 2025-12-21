using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WeaponHolder))]
[RequireComponent(typeof(Damageable))]
public class SpiderController : ChasingMob2D
{
    [Header("Spider Behaviour")]
    [SerializeField] private float aggroRange = 5f;    
    [SerializeField] private float disengageRange = 8f;     
    [SerializeField] private float attackRange = 1.2f;       
    [SerializeField] private float attackCloseBuffer = 0.35f; 
    [SerializeField, Range(0f, 1f)] private float lowHealthFraction = 0.3f; 
    [SerializeField] private float retreatSafeDistance = 10f;
    [SerializeField] private float retreatSpeedMultiplier = 1.3f;

    private WeaponHolder weaponHolder;
    private Damageable damageable;
    private Animator anim; 

    private enum SpiderState { Idle, Chasing, Attacking, Retreating }
    private SpiderState state = SpiderState.Idle;
    private bool hasAggro = false;

    protected override void Awake()
    {
        base.Awake();
        weaponHolder = GetComponent<WeaponHolder>();
        damageable = GetComponent<Damageable>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!target) return;


        // Logic here to determine movement is more complex than base class so it is not called
        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        Vector2 dir = dist > 0.001f ? toTarget / dist : Vector2.zero;

        bool lowHP = damageable != null &&
            damageable.currentHP <= damageable.maxHP * lowHealthFraction;

        //  follow within range, keep following until out of range
        if (!hasAggro && dist <= aggroRange)
            hasAggro = true;
        if (hasAggro && dist > disengageRange && !lowHP)
            hasAggro = false;

        // state selection
        if (lowHP)
        {
            // run away when low on health 
            if (dist >= retreatSafeDistance)
                state = SpiderState.Idle;
            else
                state = SpiderState.Retreating;
        }
        else if (!hasAggro)
        {
            state = SpiderState.Idle;
        }
        else
        {
            if (dist <= attackRange)
                state = SpiderState.Attacking;
            else
                state = SpiderState.Chasing;
        }

        // Aim bite towards the player
        if (weaponHolder != null && dir != Vector2.zero)
            weaponHolder.SetAim(dir);

        // flip sprite
        if (sr != null && Mathf.Abs(dir.x) > 0.01f)
            sr.flipX = dir.x > 0f;
    }

    protected override void FixedUpdate()
    {
        if (!target) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        Vector2 dir = dist > 0.001f ? toTarget / dist : Vector2.zero;

        Vector2 velocity = Vector2.zero;

        switch (state)
        {
            case SpiderState.Idle:
                velocity = Vector2.zero;
                break;

            case SpiderState.Chasing:
                velocity = dir * chaseSpeed;
                break;

            case SpiderState.Attacking:
                float desiredStopDistance = Mathf.Max(0f, attackRange - attackCloseBuffer);
                if (dist > desiredStopDistance)
                {
                    velocity = dir * chaseSpeed;
                }
                else
                {
                    velocity = Vector2.zero;
                }
                if (weaponHolder != null)
                    weaponHolder.TryAttack();
                break;

            case SpiderState.Retreating:
                if (dist < retreatSafeDistance)
                    velocity = chaseSpeed * retreatSpeedMultiplier * -dir;
                else
                    velocity = Vector2.zero;
                break;
        }

        // Flip sprite to face movement direction
        if (sr != null && Mathf.Abs(velocity.x) > 0.01f)
            {
            sr.flipX = velocity.x < 0f;
        }

        if (anim != null) {
            anim.SetFloat("Speed", velocity.magnitude);
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}
