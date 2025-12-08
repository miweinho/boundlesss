using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MeleeSwing : MonoBehaviour
{
    [Header("Arc")]
    public float startAngle = -90f;
    public float endAngle = 45f;
    public float duration = 0.18f;
    public AnimationCurve arcCurve = AnimationCurve.Linear(0,0,1,1);

    [Header("Hit window (normalized 0..1)")]
    [Range(0f,1f)] public float hitStart = 0.25f;
    [Range(0f,1f)] public float hitEnd = 0.6f;

    [Header("Hitbox")]
    public Collider2D hitbox;

    // runtime values
    private bool swinging;
    private float elapsed;
    private int damage;
    private float knockback;
    private int sourceTeam;
    private Collider2D ignoredCollider;
    private HashSet<int> hitTargets = new HashSet<int>();

    void Awake()
    {
        if (hitbox == null) hitbox = GetComponent<Collider2D>();
        if (hitbox != null) hitbox.enabled = false;
        transform.localEulerAngles = new Vector3(0,0,startAngle);
    }

    void Update()
    {
        if (!swinging) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float eval = arcCurve.Evaluate(t);
        float angle = Mathf.LerpAngle(startAngle, endAngle, eval);
        transform.localEulerAngles = new Vector3(0, 0, angle);

        if (hitbox != null)
            hitbox.enabled = (t >= hitStart && t <= hitEnd);

        if (t >= 1f) EndSwing();
    }

    void EndSwing()
    {
        swinging = false;
        elapsed = 0f;
        hitTargets.Clear();
        if (hitbox != null) hitbox.enabled = false;
        if (ignoredCollider != null && hitbox != null)
        {
            Physics2D.IgnoreCollision(hitbox, ignoredCollider, false);
            ignoredCollider = null;
        }
    }

    public void StartSwing(int damage, float knockback, int sourceTeam, Collider2D ignoreCollider = null)
    {
        if (swinging) return;
        this.damage = damage;
        this.knockback = knockback;
        this.sourceTeam = sourceTeam;
        this.ignoredCollider = ignoreCollider;
        if (ignoredCollider != null && hitbox != null)
            Physics2D.IgnoreCollision(hitbox, ignoredCollider, true);
        hitTargets.Clear();
        swinging = true;
        elapsed = 0f;
        transform.localEulerAngles = new Vector3(0,0,startAngle);
        if (hitbox != null) hitbox.enabled = false;
    }

    public IEnumerator SwingRoutine(int damage, float knockback, int sourceTeam, Collider2D ignoreCollider = null)
    {
        StartSwing(damage, knockback, sourceTeam, ignoreCollider);
        while (swinging) yield return null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!swinging) return;
        if (other == null) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        var mb = target as MonoBehaviour;
        if (mb == null) return;

        int id = mb.gameObject.GetInstanceID();
        if (hitTargets.Contains(id)) return;
        hitTargets.Add(id);

        Vector2 dir = (mb.transform.position - transform.position);
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
        else dir = Vector2.right;

        target.ApplyDamage(damage, dir, knockback, sourceTeam);
    }
}