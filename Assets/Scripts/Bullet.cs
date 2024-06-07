using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        // Debug.Log($"collision transform name {collision.transform.name} - {gameObject.name}");
        Destroy(gameObject);
    }
}
