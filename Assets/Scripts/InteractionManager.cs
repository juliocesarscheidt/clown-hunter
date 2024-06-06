using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private PlayerStats playerStats;
    public List<string> tagsToInteract = new() { "Paper", "GunAmmo" };
    private AudioSource interactionAudioSource;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        interactionAudioSource = GetComponent<AudioSource>();
    }

    private void LateUpdate() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead || playerStats.isReloading) {
            return;
        }

        Vector3 middle = new(0.5F, 0.5F, 0);

        Ray ray = Camera.main.ViewportPointToRay(middle);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            float distanceToPlayer = Vector3.Distance(hit.transform.position, playerStats.transform.position);
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
                                interactionAudioSource.Play();
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
