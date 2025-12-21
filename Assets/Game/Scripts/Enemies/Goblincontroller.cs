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

    void Update()
    {
        if (!hasAggro) return;
        if (!target || weaponHolder == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        if (dist <= 0.0001f) return;

        Vector2 dir = toTarget / dist;
        weaponHolder.SetAim(dir);

        if (dist <= attackRange)
            weaponHolder.TryAttack();
    }

    protected override void FixedUpdate()
    {
        if (!hasAggro) return;
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
