using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class Poisonable : MonoBehaviour, IPoisonable
{
    private Damageable damageable;
    private Coroutine poisonRoutine;

    void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    public void ApplyPoison(int tickDamage, int tickCount, float tickInterval, int sourceTeam)
    {
        if (damageable == null) return;

        // Overwrite any existing poison with the new one
        if (poisonRoutine != null)
            StopCoroutine(poisonRoutine);

        poisonRoutine = StartCoroutine(PoisonCo(tickDamage, tickCount, tickInterval, sourceTeam));
    }

    private IEnumerator PoisonCo(int tickDamage, int tickCount, float tickInterval, int sourceTeam)
    {
        for (int i = 0; i < tickCount; i++)
        {
            damageable.ApplyDamage(tickDamage, Vector2.zero, 0f, sourceTeam);
            yield return new WaitForSeconds(tickInterval);
        }

        poisonRoutine = null;
    }
}

