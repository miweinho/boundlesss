using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(int amount, Vector2 hitDirection, float knockback, int sourceTeam);
}

