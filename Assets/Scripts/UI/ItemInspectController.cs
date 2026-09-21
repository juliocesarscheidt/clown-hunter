using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInspectController : MonoBehaviour, IDragHandler, IPointerDownHandler {
    [Header("3D Inspection Setup")]
    [SerializeField]
    private Transform inspectAnchor; // Parent GameObject for inspected item
    public float rotationSpeed = 1f;
    public bool invertY = true;
    [System.NonSerialized]
    private GameObject currentInspectedModel;

    public Image dragAreaImage;
    public TextMeshProUGUI exitInspectText;
    public Light inspectLight;

    private Quaternion initialInspectAnchorRot;

    void Awake() {
        initialInspectAnchorRot = inspectAnchor.transform.rotation;
    }

    private void SetProps(bool enabled) {
        dragAreaImage.enabled = enabled;
        exitInspectText.gameObject.SetActive(enabled);
        inspectLight.enabled = enabled;
    }

    public void InspectItem(GameObject itemPrefab) {
        SetProps(true);

        if (currentInspectedModel != null) {
            Destroy(currentInspectedModel);
        }

        if (itemPrefab != null && inspectAnchor != null) {
            currentInspectedModel = Instantiate(itemPrefab, inspectAnchor);
            currentInspectedModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 0f, 0f));
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (inspectAnchor == null) return;

        // Extract mouse displacement during drag
        float rotX = eventData.delta.x * rotationSpeed;
        float rotY = eventData.delta.y * rotationSpeed * (invertY ? 1 : -1);

        // Rotate the inspect anchor around World Up (Y) and Camera Right (X)
        inspectAnchor.Rotate(Vector3.up, -rotX, Space.World);
        inspectAnchor.Rotate(Vector3.right, rotY, Space.World);
    }

    public void OnPointerDown(PointerEventData eventData) {
        // Required interface implementation to register drag events reliably
    }

    public void CloseInspectView() {
        SetProps(false);

        inspectAnchor.transform.rotation = initialInspectAnchorRot;

        if (currentInspectedModel != null) {
            Destroy(currentInspectedModel);
        }
    }
}
