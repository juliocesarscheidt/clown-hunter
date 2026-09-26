using UnityEngine;

public class FlashlightManager : MonoBehaviour
{
    public static FlashlightManager Instance { get; private set; }

    private PlayerStats playerStats;

    public AudioSource flashlightAudioSource;
    public Light flashlight;
    public bool flashlightEnabledAtStart = true;

    public float timeToSwitchOnOff = 0.25f;
    private float timer = 0f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Start() {
        timer = timeToSwitchOnOff;

        if (flashlight != null) {
            flashlight.enabled = flashlightEnabledAtStart;
        }
    }

    void Update() {
        if (!GlobalGameplayManager.Instance.IsGameplayActive) {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= timeToSwitchOnOff) {
            if (Input.GetAxis("JoystickHorizontalButtons") == -1 || Input.GetButtonDown("Flashlight")) {
                Toggle(!flashlight.enabled);
                timer = 0;
            }
        }
    }

    public void Toggle(bool enabled) {
        flashlight.enabled = enabled;
        flashlightAudioSource.Play();
    }

    public void TurnOn() {
        Toggle(true);
    }

    public void TurnOff() {
        Toggle(false);
    }
}
