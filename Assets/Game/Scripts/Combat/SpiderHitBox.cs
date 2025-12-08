using UnityEngine;

public class SpiderHitbox : Hitbox
{
    [Header("Poison")]
    [SerializeField] private bool applyPoison = true;
    [SerializeField] private int poisonDamagePerTick = 1;
    [SerializeField] private int poisonTickCount = 3;
    [SerializeField] private float poisonTickInterval = 0.5f;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Base damage application
        base.OnTriggerEnter2D(other);

        // Extra poison effect
        if (applyPoison && other.TryGetComponent<IPoisonable>(out var poisonable))
        {
            poisonable.ApplyPoison(
                poisonDamagePerTick,
                poisonTickCount,
                poisonTickInterval,
                sourceTeam
            );
        }
    }
}

