using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    private WeaponData data;
    private IWeaponUser user;
    private AudioSource audioSource;
    private float nextUseTime;
    private WeaponHolder holder;

    public void Initialize(WeaponData data, IWeaponUser user, AudioSource audio)
    {
        this.data = data;
        this.user = user;
        this.audioSource = audio;
        holder = (user as Component)?.GetComponent<WeaponHolder>();
    }

    public bool TryAttack()
    {
        if (data == null) return false;

        // Cooldown handling
        float cd = data.cooldown;
        if (holder && holder.attackSpeedMultiplier > 0f)
            cd = cd / holder.attackSpeedMultiplier;

        if (Time.time < nextUseTime) return false;
        nextUseTime = Time.time + cd;

        // Trigger attack animation if available
        if (user.Animator != null)
            user.Animator.SetTrigger("Attack");

        // Choose behaviour by weapon kind
        switch (data.kind)
        {
            case WeaponKind.Melee:
                DoMelee();
                break;

            case WeaponKind.Ranged:
                DoRanged();
                break;

            case WeaponKind.Bite:
                DoBite();
                break;

            default:
                // Fallback to melee
                DoMelee();
                break;
        }

        if (data.sfx != null)
            audioSource?.PlayOneShot(data.sfx);

        return true;
    }


    private void DoMelee()
    {
        if (!data.meleeHitboxPrefab || user.HandTransform == null) return;

        var hitboxGO = Instantiate(data.meleeHitboxPrefab, user.HandTransform.position, Quaternion.identity, user.HandTransform);
        hitboxGO.transform.localPosition += Vector3.right * 0.5f;
        var hitbox = hitboxGO.GetComponent<Hitbox>();

        int dmg = data.damage;
        if (holder) dmg = Mathf.RoundToInt(dmg * holder.damageMultiplier);
    
        Collider2D shooterCol = (user as Component)?.GetComponent<Collider2D>();
        hitbox.Configure(dmg, data.knockback, user.Team, user.AimDirection, shooterCol);
    }

    private void DoRanged()
    {
        if (!data.projectilePrefab) return;

        var dir = data.directionalAim ? (user.AimDirection.sqrMagnitude > 0.001f ? user.AimDirection.normalized : Vector2.right)
                                      : Vector2.right;

        var projGO = Instantiate(data.projectilePrefab, user.HandTransform.position, Quaternion.identity);
        var proj = projGO.GetComponent<Projectile>();
        Collider2D shooterCol = (user as Component).GetComponent<Collider2D>();

        int dmg = data.damage;
        if (holder) dmg = Mathf.RoundToInt(dmg * holder.damageMultiplier);
    
        proj.Fire(dir, dmg, data.knockback, user.Team, data.range, shooterCol);

        if (data.muzzleVfxPrefab)
            Instantiate(data.muzzleVfxPrefab, user.HandTransform.position, Quaternion.identity);
    }


    private void DoBite()
    {
        if (!data.meleeHitboxPrefab || user.HandTransform == null) return;

        // Spawn the bite hitbox at the hand/mouth
        var hitboxGO = Instantiate(
            data.meleeHitboxPrefab,
            user.HandTransform.position,
            Quaternion.identity,
            user.HandTransform
        );

        // Small offset forward - unsure if worth keeping or not 
        // hitboxGO.transform.localPosition += Vector3.right * 0.5f;

        int dmg = data.damage;
        if (holder)
            dmg = Mathf.RoundToInt(dmg * holder.damageMultiplier);

        Collider2D shooterCol = (user as Component)?.GetComponent<Collider2D>();

        var hitbox = hitboxGO.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.Configure(dmg, data.knockback, user.Team, user.AimDirection, shooterCol);
        }
    }

}
