using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    private WeaponData data;
    private IWeaponUser user;
    private AudioSource audioSource;
    private float nextUseTime;

    public void Initialize(WeaponData data, IWeaponUser user, AudioSource audio)
    {
        this.data = data;
        this.user = user;
        this.audioSource = audio;
    }

    public bool TryAttack()
    {
        if (Time.time < nextUseTime || data == null) return false;
        nextUseTime = Time.time + data.cooldown;

        //user.Animator?.SetTrigger("Attack");

        if (data.kind == WeaponKind.Melee) DoMelee();
        else DoRanged();

        if (data.sfx && audioSource) audioSource.PlayOneShot(data.sfx);
        return true;
    }

    private void DoMelee()
    {
        if (!data.meleeHitboxPrefab || user.HandTransform == null) return;

        var hitboxGO = Instantiate(data.meleeHitboxPrefab, user.HandTransform.position, Quaternion.identity, user.HandTransform);
        hitboxGO.transform.localPosition += Vector3.right * 0.5f;
        var hitbox = hitboxGO.GetComponent<Hitbox>();
        Collider2D shooterCol = (user as Component)?.GetComponent<Collider2D>();
        hitbox.Configure(data.damage, data.knockback, user.Team, user.AimDirection, shooterCol);
    }

    private void DoRanged()
    {
        if (!data.projectilePrefab) return;

        var dir = data.directionalAim ? (user.AimDirection.sqrMagnitude > 0.001f ? user.AimDirection.normalized : Vector2.right)
                                      : Vector2.right;

        var projGO = Instantiate(data.projectilePrefab, user.HandTransform.position, Quaternion.identity);
        var proj = projGO.GetComponent<Projectile>();
        Collider2D shooterCol = (user as Component).GetComponent<Collider2D>();
        proj.Fire(dir, data.damage, data.knockback, user.Team, data.range, shooterCol);

        if (data.muzzleVfxPrefab)
            Instantiate(data.muzzleVfxPrefab, user.HandTransform.position, Quaternion.identity);
    }
}
