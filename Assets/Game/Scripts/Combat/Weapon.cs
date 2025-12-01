using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    private WeaponData data;
    private IWeaponUser user;
    private AudioSource audioSource;
    private float nextUseTime;
    private WeaponHolder holder;
    private Animator weaponVisualAnimator;

    public void Initialize(WeaponData data, IWeaponUser user, AudioSource audio)
    {
        this.data = data;
        this.user = user;
        this.audioSource = audio;
        holder = (user as Component)?.GetComponent<WeaponHolder>();
        
        var userComp = user as Component;
        if (userComp != null)
        {
            var vm = userComp.GetComponentInChildren<WeaponVisualManager>();
            if (vm != null && data != null)
            {
                vm.EquipVisual(data.visualPrefab);
                // cache animator reference for ranged/bow animations
                weaponVisualAnimator = vm.BowAnimator;
            }
        }
    }

    public bool TryAttack()
    {
    if (data == null) return false;

    float cd = data.cooldown;
    if (holder && holder.attackSpeedMultiplier > 0f)
        cd = cd / holder.attackSpeedMultiplier;

    if (Time.time < nextUseTime) return false;
    nextUseTime = Time.time + cd;

    if (data.kind == WeaponKind.Melee)
    {
        DoMelee();
        return true;
    }

    if (weaponVisualAnimator != null)
    {
        weaponVisualAnimator.SetTrigger("Fire");
    }

    // Player body animation (optional)
    if (user.Animator != null)
        user.Animator.SetTrigger("Attack");

    return true;
}

    private void DoMelee()
    {
        var userComp = user as Component;
        if (userComp == null) return;

        var swing = userComp.GetComponentInChildren<MeleeSwing>();
        if (swing != null)
        {
            Collider2D shooterCol = (user as Component)?.GetComponent<Collider2D>();
            swing.StartSwing(data.damage, data.knockback, user.Team, shooterCol);
            // Play swing sound effect
            if (data.sfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(data.sfx);
            }
            return;
        }
        // fallback: existing hitbox-prefab flow (if you still use it)
        if (!data.meleeHitboxPrefab || user.HandTransform == null) return;
        var hitboxGO = Instantiate(data.meleeHitboxPrefab, user.HandTransform.position, Quaternion.identity, user.HandTransform);
        var hitbox = hitboxGO.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            Collider2D shooterCol = (user as Component)?.GetComponent<Collider2D>();
            hitbox.Configure(data.damage, data.knockback, user.Team, user.AimDirection, shooterCol);
        }
    }

    private void DoRanged()
    {
        if (!data.projectilePrefab) return;
        if (holder)
        {
            if (data.muzzleVfxPrefab)
            {
                Instantiate(data.muzzleVfxPrefab, user.HandTransform.position, Quaternion.identity);
            }

            GameObject projGO = Instantiate(data.projectilePrefab, user.HandTransform.position, Quaternion.identity);
            
            // Set projectile layer based on team
            int projectileLayer = (user.Team == 0) ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");
            projGO.layer = projectileLayer;
            
            Projectile proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                var dir = data.directionalAim ? (user.AimDirection.sqrMagnitude > 0.001f ? user.AimDirection.normalized : Vector2.right)
                                      : Vector2.right;

                int dmg = data.damage;
                if (holder) dmg = Mathf.RoundToInt(dmg * holder.damageMultiplier);
            
                proj.Fire(dir, dmg, data.knockback, user.Team, data.range, (user as Component).GetComponent<Collider2D>());

                if (data.sfx != null)
                    audioSource?.PlayOneShot(data.sfx);
            }
        }
    }

public void OnBowRelease()
{
    // only ranged
    if (data == null || data.kind != WeaponKind.Ranged)
        return;

    DoRanged();

    if (data.sfx != null)
        audioSource?.PlayOneShot(data.sfx);
}

}