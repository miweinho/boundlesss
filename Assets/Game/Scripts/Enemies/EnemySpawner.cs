using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public EnemySpawnProfile profile;
    public Transform[] spawnPoints;
    [SerializeField] private Transform playerTransform;

    [Header("Control")]
    public int initialCount = 5;
    public float respawnDelay = 0f; // set >0 if you want timed spawns
    public int maxAlive = 20;

    private readonly List<GameObject> alive = new();
    private float _timer = 0f;

    void Start()
    {
        int toSpawn = Mathf.Min(initialCount, maxAlive);
        for (int i = 0; i < toSpawn; i++) SpawnOne();
    }

    void Update()
    {
        // cleanup destroyed enemies from list
        alive.RemoveAll(go => go == null);

        // optional timed spawning
        if (respawnDelay > 0f && alive.Count < maxAlive)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = respawnDelay;
                SpawnOne();
            }
        }
    }

    void SpawnOne()
    {
        if (profile == null || profile.enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;
        if (alive.Count >= maxAlive) return;

        var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var enemy = Instantiate(profile.enemyPrefab, point.position, Quaternion.identity);
        alive.Add(enemy);

        // --- Randomize stats ---

        // Health
        var dmgbl = enemy.GetComponent<Damageable>();
        if (dmgbl)
        {
            int hp = Random.Range(profile.healthRange.x, profile.healthRange.y + 1);
            dmgbl.SetMaxHealth(hp, true); // <-- use the new API instead of reflection
        }

        // Move speed + target
        var ai = enemy.GetComponent<Simple_Chaser>();
        if (ai)
        {
            float spd = Random.Range(profile.speedRange.x, profile.speedRange.y);
            // either direct field or setter depending on what you chose:
            ai.SetMoveSpeed(spd); // or ai.SetMoveSpeed(spd);
            if (playerTransform) ai.SetTarget(playerTransform); // or ai.target = playerTransform;
        }

        // Weapon multipliers + random weapon
        var holder = enemy.GetComponent<WeaponHolder>();
        if (holder)
        {
            holder.damageMultiplier = Random.Range(profile.damageMultRange.x, profile.damageMultRange.y);
            holder.attackSpeedMultiplier = Random.Range(profile.atkSpeedMultRange.x, profile.atkSpeedMultRange.y);

            if (profile.possibleWeapons != null && profile.possibleWeapons.Length > 0)
            {
                var w = profile.possibleWeapons[Random.Range(0, profile.possibleWeapons.Length)];
                holder.Equip(w); // equips logic + (optionally) visuals if you wired that earlier
            }
        }

        // Remove from list on death and destroy the object
        if (dmgbl)
        {
            var eRef = enemy;
            dmgbl.OnDie += () =>
            {
                alive.Remove(eRef);
                if (eRef) Destroy(eRef);
            };
        }
    }
}