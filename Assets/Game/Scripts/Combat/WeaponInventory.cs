using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private WeaponHolder holder;

    [Header("Starting Loadout (Inspector)")]
    [SerializeField] private WeaponData[] loadout; // starting weapons

    [Header("Runtime")]
    [SerializeField] private bool equipFirstWeaponOnStart = true;

    private readonly List<WeaponData> _weapons = new();
    private int index = 0;

    void Start()
    {
        // build runtime list from inspector loadout
        _weapons.Clear();
        if (loadout != null)
        {
            for (int i = 0; i < loadout.Length; i++)
            {
                var w = loadout[i];
                if (w != null && !_weapons.Contains(w))
                    _weapons.Add(w);
            }
        }

        if (equipFirstWeaponOnStart && holder && _weapons.Count > 0)
        {
            index = Mathf.Clamp(index, 0, _weapons.Count - 1);
            holder.Equip(_weapons[index]);
        }
    }

    public bool HasWeapon(WeaponData weapon) => weapon != null && _weapons.Contains(weapon);

    /// <summary>Adds a weapon to the inventory at runtime. Optionally equips it.</summary>
    public bool AddWeapon(WeaponData weapon, bool equipOnAdd = true, bool equipOnlyIfEmpty = false)
    {
        if (weapon == null) return false;

        if (!_weapons.Contains(weapon))
            _weapons.Add(weapon);

        // make it the "current" weapon
        index = _weapons.IndexOf(weapon);

        if (!holder) return true;

        if (equipOnAdd)
        {
            if (equipOnlyIfEmpty && holder.HasEquippedWeapon)
                return true;

            holder.Equip(weapon);
        }

        return true;
    }

    public void NextWeapon()
    {
        if (!holder || _weapons.Count == 0) return;
        index = (index + 1) % _weapons.Count;
        holder.Equip(_weapons[index]);
    }

    public void PreviousWeapon()
    {
        if (!holder || _weapons.Count == 0) return;
        index = (index - 1 + _weapons.Count) % _weapons.Count;
        holder.Equip(_weapons[index]);
    }
}
