using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectible : MonoBehaviour
{

    private int healing;
    private int respawnTime;

    public void Configure(int healing, int respawnTime)
    {
        this.healing = healing;
        this.respawnTime = respawnTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.Heal(healing);
        }
            Destroy(gameObject);
    }
    
}
