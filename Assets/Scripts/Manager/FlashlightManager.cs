using UnityEngine;

public class FlashlightManager : MonoBehaviour
{
    private PlayerStats playerStats;

    public AudioSource flashlightAudioSource;
    public Light flashlight;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        if (flashlight != null) {
            flashlight.enabled = false;
        }
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            flashlightAudioSource.Play();
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
