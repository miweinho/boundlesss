using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualManager : MonoBehaviour
{
    [SerializeField] private Transform hand;
    private GameObject currentVisual;

    public void EquipVisual(GameObject visualPrefab)
    {
        ClearVisual();
        if (!hand || !visualPrefab) return;

        currentVisual = Instantiate(visualPrefab, hand);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
    }

    public void ClearVisual()
    {
        if (currentVisual) Destroy(currentVisual);
        currentVisual = null;
    }
}