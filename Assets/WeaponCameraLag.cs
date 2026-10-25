using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCameraLag : MonoBehaviour
{
    [Tooltip("Drag your Main Camera here.")]
    public Transform mainCameraTransform;

    [Header("Weapon Lag Settings")]
    [Tooltip("Higher = snappier gun movement. Lower = heavier, more delayed lag feel.")]
    public float rotationSmoothness = 20f;

    void LateUpdate() {
        if (mainCameraTransform == null) return;

        // Smoothly Slerp the rotation to lag behind the main camera's rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, mainCameraTransform.rotation, rotationSmoothness * Time.deltaTime);
    }
}
