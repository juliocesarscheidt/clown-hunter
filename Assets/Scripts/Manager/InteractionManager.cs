using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private PlayerStats playerStats;
    public List<string> tagsToInteract = new() { TagsController.Paper, TagsController.GunAmmo, TagsController.FirstAid };
    private AudioSource interactionAudioSource;
    [SerializeField]
    private List<Interactable> interactables = new();
    
    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        interactionAudioSource = GetComponent<AudioSource>();

        foreach (var obj in FindObjectsByType(typeof(Interactable), FindObjectsSortMode.None)) {
            interactables.Add(obj as Interactable);
        }
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void LateUpdate() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead || playerStats.isReloading) {
            return;
        }

        Vector3 center = new(0.5F, 0.5F, 0);
        Ray ray = Camera.main.ViewportPointToRay(center);

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            float distanceToPlayer = Vector3.Distance(hit.transform.position, playerStats.transform.position);
            if (distanceToPlayer > 5f) {
                HudManager.Instance.HidePressEObject();
                DisableAllOutlines();

                return;
            }

            if (tagsToInteract.Exists(t => hit.collider.CompareTag(t))) {
                foreach (var tag in tagsToInteract) {
                    if (hit.collider.CompareTag(tag)) {
                        // display UIU message
                        HudManager.Instance.ShowPressEObject();

                        if (hit.transform.gameObject.TryGetComponent(out Interactable obj)) {
                            // show object outline
                            if (!obj.isOutlineEnabled) {
                                obj.EnableOutline();
                            }

                            if (Input.GetKeyDown(KeyCode.E)) {
                                obj.Collect();
                                interactionAudioSource.Play();

                                int index = interactables.FindIndex(interactable => interactable == obj);
                                if (index >= 0) interactables.RemoveAt(index);

                                Destroy(hit.transform.gameObject);
                            }
                        }
                    }
                }
            
            } else {
                HudManager.Instance.HidePressEObject();
                DisableAllOutlines();
            }
        }
    }

    private void DisableAllOutlines() {
        foreach (var interactable in interactables) {
            if (interactable.isOutlineEnabled) {
                interactable.DisableOutline();
            }
        }
    }

    public void AddInteractable(Interactable interactable) {
        interactables.Add(interactable);
    }
}
