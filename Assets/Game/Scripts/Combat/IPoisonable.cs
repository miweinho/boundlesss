using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoisonable
{
    void ApplyPoison(int tickDamage, int tickCount, float tickInterval, int sourceTeam);
}
