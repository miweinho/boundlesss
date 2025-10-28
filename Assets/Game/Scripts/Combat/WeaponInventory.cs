using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private WeaponHolder holder;
    [SerializeField] private WeaponData[] loadout; // assign in Inspector
    private int index = 0;

    void Start()
    {
        if (holder && loadout != null && loadout.Length > 0)
            holder.Equip(loadout[index]);
    }

    public void NextWeapon()
    {
        if (!holder || loadout == null || loadout.Length == 0) return;
        index = (index + 1) % loadout.Length;
        holder.Equip(loadout[index]);
    }

    public void PreviousWeapon()
    {
        if (!holder || loadout == null || loadout.Length == 0) return;
        index = (index - 1 + loadout.Length) % loadout.Length;
        holder.Equip(loadout[index]);
    }
}
