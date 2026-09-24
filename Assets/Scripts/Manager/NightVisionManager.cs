using UnityEngine;

public class NightVisionManager : MonoBehaviour
{
    public static NightVisionManager Instance { get; private set; }

    private PlayerStats playerStats;
    
    public AudioSource nightVisionAudioSource;
    private bool nightVisionIsOn = false;

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
    }

    void Update() {
        if (InventoryManager.Instance.IsShowingInventory || HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            if (!HudManager.Instance.IsRunningGame || playerStats.isDead) {
                PostProcessingManager.Instance.SetDefaultProfile();
            }
            
            return;
        }

        timer += Time.deltaTime;

        if (timer >= timeToSwitchOnOff) {
            if (!nightVisionAudioSource.isPlaying && (
                Input.GetAxis("JoystickHorizontalButtons") == 1 || Input.GetButtonDown("Nightvision")
            )) {
                Toggle(!nightVisionIsOn);
                timer = 0;
            }
        }
    }

    public void Toggle(bool enabled) {
        nightVisionIsOn = enabled;
        if (nightVisionIsOn) {
            nightVisionAudioSource.Play();
        }
        if (nightVisionIsOn) {
            PostProcessingManager.Instance.SetNightVisionProfile();
        } else {
            PostProcessingManager.Instance.SetDefaultProfile();
        }
    }

    public void TurnOn() {
        Toggle(true);
    }

    public void TurnOff() {
        Toggle(false);
    }
}
