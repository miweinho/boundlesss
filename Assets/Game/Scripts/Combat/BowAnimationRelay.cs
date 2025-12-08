using UnityEngine;

public class BowAnimationRelay : MonoBehaviour
{
    // Animation event calls this
    public void OnBowRelease()
    {
        // Find weapon dynamically instead of caching in Awake
        var hand = transform.parent?.parent;
        var weapon = hand?.GetComponentInChildren<Weapon>();
        weapon?.OnBowRelease();
    }
}
