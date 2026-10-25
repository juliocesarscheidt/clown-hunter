using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private AudioSource interactionAudioSource;

    [Header("Interactables")]
    [SerializeField]
    private List<Interactable> interactables = new();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        interactionAudioSource = GetComponent<AudioSource>();
    }

    void Start() {
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
                SetPressInteractTextToMouseKeyboard();
            } else {
                SetPressInteractTextToXBoxJoystick(); // XInputControllerWindows
            }
        }
    }

    private void LateUpdate() {
        if (HudManager.Instance.IsRunningGame) {
            if (GlobalGameplayManager.Instance.IsGameplayPaused) {
                interactionAudioSource.Pause();
            } else {
                interactionAudioSource.UnPause();
            }
        }
    }

    public void SetPressInteractTextToMouseKeyboard() {
        foreach (var interactable in interactables) {
            interactable.SetInteractText("[F]");
        }
    }

    public void SetPressInteractTextToXBoxJoystick() {
        foreach (var interactable in interactables) {
            interactable.SetInteractText("[Y]");
        }
    }

    public void AddInteractable(Interactable obj) {
        interactables.Add(obj);
    }

    public void RemoveInteractable(Interactable obj) {
        int index = interactables.FindIndex(interactable => interactable == obj);
        if (index >= 0) interactables.RemoveAt(index);
    }

    public void PlayCollectAudio() {
        interactionAudioSource.Play();
    }
}
