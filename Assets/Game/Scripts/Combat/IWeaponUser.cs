using UnityEngine;

public interface IWeaponUser
{
    Transform HandTransform { get; }   // where weapon/hitbox spawns
    int Team { get; }                  // 0 = player, 1 = enemies (for friendly fire rules)
    Vector2 AimDirection { get; }      // e.g., from input or AI
    Animator Animator { get; }         // optional, can be null
}
