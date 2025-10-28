using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimFollower : MonoBehaviour
{
    [SerializeField] private WeaponHolder holder;
    [SerializeField] private bool useRightAxis = true;

    void Reset()
    {
        if (!holder) holder = GetComponentInParent<WeaponHolder>();
    }

    void LateUpdate()
    {
        if (!holder) return;
        var dir = holder.AimDirection;
        if (dir.sqrMagnitude < 0.0001f) return;

        if (useRightAxis)
            transform.right = dir;
        else
            transform.up = dir;
    }
}
