using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponHolder : MonoBehaviour, IWeaponUser
{
    [Header("Runtime Combat Multipliers")]
    [Range(0.25f, 4f)] public float damageMultiplier = 1f;
    [Range(0.25f, 4f)] public float attackSpeedMultiplier = 1f;
    [SerializeField] private Transform handTransform;
    [SerializeField] private int team = 0; // player=0 by default
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private WeaponVisualManager visualManager;

    public Transform HandTransform => handTransform;
    public int Team => team;
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public Animator Animator => animator;

    private AudioSource audioSource;
    private Weapon equipped;

    public bool HasEquippedWeapon => equipped != null;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (startingWeapon) Equip(startingWeapon);
    }

    public void SetAim(Vector2 dir)
    {
        AimDirection = dir;
        
        // Rotate the hand (and weapon visual) to face aim direction
        if (handTransform != null && dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            handTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Equip(WeaponData data)
    {
        Unequip();

        if (!data) { visualManager?.ClearVisual(); return; }

        // spawn weapon logic (you already have this)
        var go = new GameObject($"Weapon_{data.displayName}");
        go.transform.SetParent(handTransform, false);
        equipped = go.AddComponent<Weapon>();
        equipped.Initialize(data, this, audioSource);

        // spawn visual
        //visualManager?.EquipVisual(data.visualPrefab);
    }

    public void Unequip()
    {
        if (equipped)
        {
            Destroy(equipped.gameObject);
            equipped = null;
        }
        visualManager?.ClearVisual();
    }

    public bool TryAttack() => equipped && equipped.TryAttack();
}
