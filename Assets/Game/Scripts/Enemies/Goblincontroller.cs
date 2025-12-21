using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WeaponHolder))]
[RequireComponent(typeof(Damageable))]
public class Goblincontroller : ChasingMob2D
{
    [Header("Combat")]
    [SerializeField, Min(0f)] private float attackRange = 1f;
    [SerializeField] private WeaponHolder weaponHolder;

    [Header("Targeting")]
    [SerializeField] private string targetTagOverride = "Player";

    [Header("Aggro")]
    [Tooltip("Distance at which this mob will start chasing/attacking the player when aggressive.")]
    [SerializeField] private float aggroActivationDistance = 8f;

    private Damageable damageable;
    private bool hasAggro;

    protected override void Awake()
    {
        targetTag = targetTagOverride;
        base.Awake();
        if (!weaponHolder) weaponHolder = GetComponent<WeaponHolder>();
        damageable = GetComponent<Damageable>();
        if (damageable != null)
            damageable.OnHealthChanged += HandleHealthChanged;
    }

    private void OnEnable()
    {
        // subscribe to the global damage event
        Damageable.OnAnyDamaged += HandleAnyDamaged;
    }

    private void OnDisable()
    {
        Damageable.OnAnyDamaged -= HandleAnyDamaged;
    }

    // Global damage handler: mark mob as aggressive when any damage happens.
    // Signature matches Damageable.OnAnyDamaged(Damageable, GameObject)
    private void HandleAnyDamaged(Damageable victim)
    {
        // become aggressive (movement/attack will only occur if player is within activation distance)
        hasAggro = true;
    }

    void Update()
    {
        if (!hasAggro) return;
        if (!target || weaponHolder == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        if (dist <= 0.0001f) return;

        // Only aim / attack if within activation distance
        if (dist <= aggroActivationDistance)
        {
            Vector2 dir = toTarget / dist;
            weaponHolder.SetAim(dir);

            if (dist <= attackRange)
                weaponHolder.TryAttack();
        }
    }

    protected override void FixedUpdate()
    {
        // Only chase (movement) if aggressive AND player within activation distance
        if (!hasAggro || target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > aggroActivationDistance) return;

        base.FixedUpdate();
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (current < max)
            hasAggro = true;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnHealthChanged -= HandleHealthChanged;
    }
}
