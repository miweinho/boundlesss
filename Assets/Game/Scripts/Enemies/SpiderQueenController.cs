using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WeaponHolder))]
[RequireComponent(typeof(Damageable))]
public class SpiderQueenController : ChasingMob2D
{
    [Header("Aggro / Ranges")]
    [SerializeField] private float aggroRange = 10f;        
    [SerializeField] private float disengageRange = 13f;  
    [SerializeField] private float rangedAttackRange = 8f; 
    [SerializeField] private float biteAttackRange = 2f;   

    [Header("Weapons")]
    [SerializeField] private WeaponData bowWeapon;          
    [SerializeField] private WeaponData biteWeapon;       

    [Header("Enrage (Half HP)")]
    [SerializeField, Range(0f, 1f)]
    private float enrageHealthFraction = 0.5f;              
    [SerializeField] private float enrageSpeedMultiplier = 1.5f;
    [SerializeField] private float enrageDamageMultiplier = 1.5f;
    [SerializeField] private float enrageAttackSpeedMultiplier = 1.5f;

    private WeaponHolder weaponHolder;
    private Damageable damageable;

    private float baseChaseSpeed;
    private float baseDamageMultiplier;
    private float baseAttackSpeedMultiplier;

    private bool enraged = false;
    private bool hasAggro = false;
    private bool loggedMissingDependencies = false;
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private enum QueenState { Idle, Chasing, RangedAttacking, MeleeAttacking }
    private QueenState state = QueenState.Idle;

    private WeaponData currentWeapon;

    protected override void Awake()
    {
        base.Awake(); 

        weaponHolder = GetComponent<WeaponHolder>();  
        damageable = GetComponent<Damageable>();

        if (weaponHolder != null && damageable != null)
            weaponHolder.SetTeam(damageable.team);

        baseChaseSpeed = chaseSpeed;              

        if (weaponHolder != null)
        {
            baseDamageMultiplier = weaponHolder.damageMultiplier;
            baseAttackSpeedMultiplier = weaponHolder.attackSpeedMultiplier;
        }
        else
        {
            baseDamageMultiplier = 1f;
            baseAttackSpeedMultiplier = 1f;
        }

        // Start with bow equipped 
        if (weaponHolder != null && bowWeapon != null)
        {
            weaponHolder.Equip(bowWeapon);
            currentWeapon = bowWeapon;
        }
    }

    void Update()
    {
        if (!target || weaponHolder == null || damageable == null)
        {
            if (!loggedMissingDependencies)
            {
                Debug.LogWarning($"SpiderQueen missing refs - target: {(target ? target.name : "null")}, weaponHolder: {(weaponHolder != null)}, damageable: {(damageable != null)}", this);
                loggedMissingDependencies = true;
            }
            return;
        }

        // Enrage check 
        if (!enraged && damageable.currentHP <= damageable.maxHP * enrageHealthFraction)
        {
            enraged = true;
            chaseSpeed = baseChaseSpeed * enrageSpeedMultiplier;
            weaponHolder.damageMultiplier = baseDamageMultiplier * enrageDamageMultiplier;
            weaponHolder.attackSpeedMultiplier = baseAttackSpeedMultiplier * enrageAttackSpeedMultiplier;
        }

        // Direction & distance to player
        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;
        Vector2 dir = dist > 0.001f ? toTarget / dist : Vector2.zero;

        if (!hasAggro && dist <= aggroRange)
            hasAggro = true;
        if (hasAggro && dist > disengageRange)
            hasAggro = false;

        if (!hasAggro)
        {
            state = QueenState.Idle;
        }
        else
        {
            // Decide attack mode based on the distance
            if (dist <= biteAttackRange)
            {
                state = QueenState.MeleeAttacking;
                EnsureWeapon(biteWeapon);
            }
            else if (dist <= rangedAttackRange)
            {
                state = QueenState.RangedAttacking;
                EnsureWeapon(bowWeapon);
            }
            else
            {
                state = QueenState.Chasing;
                EnsureWeapon(bowWeapon);
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SpiderQueen] dist={dist:F2} state={state} hasAggro={hasAggro} " +
                      $"weapon={(currentWeapon ? currentWeapon.name : "null")} " +
                      $"biteRange={biteAttackRange} rangedRange={rangedAttackRange}", this);
        }
        
        if (dir != Vector2.zero)
        {
            weaponHolder.SetAim(dir); 
            if (sr != null && Mathf.Abs(dir.x) > 0.01f)
                sr.flipX = dir.x < 0f;
        }
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
            case QueenState.Idle:
                velocity = Vector2.zero;
                break;

            case QueenState.Chasing:
                velocity = dir * chaseSpeed;
                break;

            case QueenState.RangedAttacking:
                velocity = dir * chaseSpeed;
                if (weaponHolder != null)
                    weaponHolder.TryAttack();
                break;

            case QueenState.MeleeAttacking:
                velocity = Vector2.zero;
                if (weaponHolder != null)
                    weaponHolder.TryAttack(); 
                break;
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void EnsureWeapon(WeaponData desired)
    {
        if (!weaponHolder || desired == null) return;
        if (currentWeapon == desired) return;

        weaponHolder.Equip(desired); 
        currentWeapon = desired;
        if (enableDebugLogs)
            Debug.Log($"[SpiderQueen] Equipped weapon: {desired.name}", this);
    }
}
