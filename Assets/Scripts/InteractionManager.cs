using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private GameObject player;
    private PlayerStats playerStats;
    public List<string> tagsToInteract = new List<string> { "Paper", "GunAmmo" };

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();
    }

    private void LateUpdate() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }

        Vector3 middle = new Vector3(0.5F, 0.5F, 0);

        Ray ray = Camera.main.ViewportPointToRay(middle);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            float distanceToPlayer = Vector3.Distance(hit.transform.position, player.transform.position);
            if (distanceToPlayer > 5f) {
                HudManager.Instance.HidePressEObject();
                return;
            }

            if (tagsToInteract.Exists(t => hit.collider.CompareTag(t))) {
                foreach (var tag in tagsToInteract) {
                    if (hit.collider.CompareTag(tag)) {
                        HudManager.Instance.ShowPressEObject();
                        if (Input.GetKeyDown(KeyCode.E)) {
                            if (hit.transform.gameObject.TryGetComponent(out Interactable obj)) {
                                obj.Collect();
                                Destroy(hit.transform.gameObject);
                            }
                        }
                    }
                }
            
            } else {
                HudManager.Instance.HidePressEObject();
            }
        }
    }
}
