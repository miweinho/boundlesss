using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnProfile
{
    public GameObject enemyPrefab;        // base enemy prefab (with WeaponHolder, Damageable, AI)
    public WeaponData[] possibleWeapons;  // assign Sword/Bow/etc.
    public Vector2Int healthRange = new Vector2Int(40, 120);
    public Vector2 speedRange = new Vector2(1.5f, 3.5f);         // AI movement speed
    public Vector2 damageMultRange = new Vector2(0.8f, 2.0f);    // per-enemy damage multiplier
    public Vector2 atkSpeedMultRange = new Vector2(0.8f, 1.8f);  // per-enemy attack speed multiplier
}