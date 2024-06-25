using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Start() {
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }
    }

    void Update() {
        transform.LookAt(targetCamera.transform, Vector3.up);
    }
}
