using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CenterItemFocusController : MonoBehaviour {
    [Header("Volume & Camera References")]
    public Volume uiVolume;
    public Camera targetCamera;

    [Header("Target Center Slot")]
    public Transform centerSlotTransform; // Slot 2 Transform

    [Header("Focus Tuning")]
    [SerializeField]
    private float focusSpeed = 12f;
    [SerializeField]
    private float focusOffset = 0f; // Fine-tune focus plane offset if needed
    private DepthOfField depthOfField;

    private void Awake() {
        if (targetCamera == null) targetCamera = Camera.main;
        // Extract Depth of Field override from the UI-layer Volume
        if (uiVolume != null && uiVolume.profile != null && !uiVolume.profile.TryGet(out depthOfField)) {
            Debug.LogWarning("Depth of Field override not found in the UI Volume Profile!");
        }
    }

    private void LateUpdate() {
        if (depthOfField == null || centerSlotTransform == null || targetCamera == null) {
            return;
        }

        // Calculate distance from camera lens along camera forward axis
        Vector3 camToTarget = centerSlotTransform.position - targetCamera.transform.position;
        float targetDistance = Vector3.Dot(camToTarget, targetCamera.transform.forward) + focusOffset;

        // Smoothly update the focus distance on the DoF post-process override
        depthOfField.focusDistance.value = Mathf.Lerp(
            depthOfField.focusDistance.value,
            targetDistance,
            Time.deltaTime * focusSpeed
        );
    }

    public void SetCenterTarget(Transform newCenterSlot) {
        centerSlotTransform = newCenterSlot;
    }
}
