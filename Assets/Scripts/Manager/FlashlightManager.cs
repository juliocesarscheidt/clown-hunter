using UnityEngine;

public class FlashlightManager : MonoBehaviour
{
    private PlayerStats playerStats;

    public AudioSource flashlightAudioSource;
    public Light flashlight;

    public float timeToSwitchOnOff = 0.25f;
    private float timer = 0f;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        timer = timeToSwitchOnOff;

        if (flashlight != null) {
            flashlight.enabled = false;
        }
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= timeToSwitchOnOff) {
            if (Input.GetAxis("JoystickHorizontalButtons") == -1 || Input.GetButtonDown("Flashlight")) {
                flashlightAudioSource.Play();
                flashlight.enabled = !flashlight.enabled;

                timer = 0;
            }
        }
    }
}
