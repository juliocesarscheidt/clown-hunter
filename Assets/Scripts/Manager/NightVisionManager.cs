using UnityEngine;

public class NightVisionManager : MonoBehaviour
{
    private PlayerStats playerStats;
    
    public AudioSource nightVisionAudioSource;
    private bool nightVisionIsOn = false;

    public float timeToSwitchOnOff = 0.25f;
    private float timer = 0f;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        timer = timeToSwitchOnOff;
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= timeToSwitchOnOff) {
            if (!nightVisionAudioSource.isPlaying && (
                Input.GetAxis("JoystickHorizontalButtons") == 1 || Input.GetButtonDown("Nightvision")
            )) {
                nightVisionIsOn = !nightVisionIsOn;
                if (nightVisionIsOn) {
                    nightVisionAudioSource.Play();
                }
                timer = 0;
            }
        }

        if (nightVisionIsOn) {
            PostProcessingManager.Instance.SetNightVisionProfile();
        } else {
            PostProcessingManager.Instance.SetDefaultProfile();
        }
    }
}
