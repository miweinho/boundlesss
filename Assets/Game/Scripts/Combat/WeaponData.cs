using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponKind { Melee, Ranged }

[CreateAssetMenu(menuName = "RPG/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public Sprite icon;

    [Header("Type")]
    public WeaponKind kind;

    [Header("Visuals (optional)")]
    public GameObject visualPrefab; // e.g., Sword_Visual, Bow_Visual

    [Header("Combat")]
    public int damage = 1;
    public float knockback = 2f;
    public float critChance = 0f;         // optional
    public float cooldown = 0.35f;        // seconds between uses
    public float range = 1.2f;            // melee reach or projectile speed scalar

    [Header("Aiming")]
    public bool directionalAim = true;    // if true, use aim direction

    [Header("Melee")]
    public GameObject meleeHitboxPrefab;  // has Hitbox component (trigger collider)

    [Header("Ranged")]
    public GameObject projectilePrefab;   // has Projectile component

    [Header("FX")]
    public AudioClip sfx;
    public GameObject swingVfxPrefab;
    public GameObject muzzleVfxPrefab;
}

