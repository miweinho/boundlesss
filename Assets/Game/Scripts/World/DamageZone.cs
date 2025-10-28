using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f; // seconds between damage ticks per target
    private int damage;
    private int respawnTime;

    //map to store which Instance was already damaged
    private Dictionary<int, float> nextDamageTime = new Dictionary<int, float>();

    public void Configure(int damage, int respawnTime)
    {
        this.damage = damage;
        this.respawnTime = respawnTime;
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (!other) return;
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            int id = other.gameObject.GetInstanceID();
            if (!nextDamageTime.TryGetValue(id, out float next) || Time.time >= next)
            {
                dmg.ApplyZoneDamage(damage);
                nextDamageTime[id] = Time.time + tickInterval;
            }

        }
    }
    
        void OnTriggerExit2D(Collider2D other)
    {
        if (!other) return;
        int id = other.gameObject.GetInstanceID();
        nextDamageTime.Remove(id);
    }

    void OnDisable()
    {
        nextDamageTime.Clear();
    }

}