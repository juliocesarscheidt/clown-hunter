using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float destroyTime = 5f;

    private void Start() {
        Destroy(gameObject, destroyTime);
    }
}
