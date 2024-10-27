using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private PlayerStats playerStats;
    public List<string> tagsToInteract = new() { TagsController.Paper, TagsController.GunAmmo, TagsController.FirstAid };
    private AudioSource interactionAudioSource;
    [SerializeField]
    private List<Interactable> interactables = new();

    [SerializeField]
    private InputActionAsset inputActionAsset;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        interactionAudioSource = GetComponent<AudioSource>();

        foreach (var obj in FindObjectsByType(typeof(Interactable), FindObjectsSortMode.None)) {
            interactables.Add(obj as Interactable);
        }

        InputSystem.onActionChange += InputActionChangeCallback;
    }

    private void InputActionChangeCallback(object obj, InputActionChange change) {
        if (change == InputActionChange.ActionPerformed) {
            InputAction receivedInputAction = (InputAction) obj;
            InputDevice lastDevice = receivedInputAction.activeControl.device;

            var isKeyboardAndMouse = lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse");
            // Debug.Log($"InputActionChangeCallback: {lastDevice.name} - {isKeyboardAndMouse}");
            if (isKeyboardAndMouse) {
                HudManager.Instance.SetPressInteractTextToMouseKeyboard();
            } else {
                // XInputControllerWindows
                HudManager.Instance.SetPressInteractTextToXBoxJoystick();
            }
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
                HudManager.Instance.HidePressInteractObject();
                DisableAllOutlines();

                return;
            }

            if (tagsToInteract.Exists(t => hit.collider.CompareTag(t))) {
                foreach (var tag in tagsToInteract) {
                    if (hit.collider.CompareTag(tag)) {
                        // display UI message
                        HudManager.Instance.ShowPressInteractObject();

                        if (hit.transform.gameObject.TryGetComponent(out Interactable obj)) {
                            // show object outline
                            if (!obj.isOutlineEnabled) obj.EnableOutline();

                            if (Input.GetButtonDown("Interact")) {
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
                HudManager.Instance.HidePressInteractObject();
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
