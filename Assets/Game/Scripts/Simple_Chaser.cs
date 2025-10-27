using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simple_Chaser : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private float attackRange = 2f;

    void Update()
    {
        if (!target) return;
        Vector2 dir = (target.position - weaponHolder.HandTransform.position).normalized;
        weaponHolder.SetAim(dir);

        if (Vector2.Distance(target.position, transform.position) <= attackRange)
            weaponHolder.TryAttack();
    }
}
