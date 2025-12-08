using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualManager : MonoBehaviour
{
    [SerializeField] private Transform hand;
    private GameObject currentVisual;
    public Animator BowAnimator {get; private set; }

    public void EquipVisual(GameObject visualPrefab)
    {
        ClearVisual();
        if (!hand || !visualPrefab) return;

        currentVisual = Instantiate(visualPrefab, hand);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;

        // cache animator on the visual (if any) for the Weapon to use
        BowAnimator = currentVisual.GetComponentInChildren<Animator>();
    }
    
    public void EquipVisual(GameObject visualPrefab, float? swingDuration = null, float? startAngle = null, float? endAngle = null)
    {
        ClearVisual();
        if (!hand || !visualPrefab) return;

        currentVisual = Instantiate(visualPrefab, hand);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;

        // cache animator on the visual (if any) so callers can read it reliably
        BowAnimator = currentVisual.GetComponentInChildren<Animator>();

        var swing = currentVisual.GetComponentInChildren<MeleeSwing>();
        if (swing != null)
        {
            if (swingDuration.HasValue) swing.duration = swingDuration.Value;
            if (startAngle.HasValue) swing.startAngle = startAngle.Value;
            if (endAngle.HasValue) swing.endAngle = endAngle.Value;
        }
    }

    public void ClearVisual()
    {
        if (currentVisual) Destroy(currentVisual);
        currentVisual = null;
    }
}