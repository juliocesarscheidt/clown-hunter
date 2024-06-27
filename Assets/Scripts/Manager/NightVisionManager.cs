using UnityEngine;

public class NightVisionManager : MonoBehaviour
{
    private PlayerStats playerStats;
    
    public AudioSource nightVisionAudioSource;
    private bool nightVisionIsOn = false;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.V) && !nightVisionAudioSource.isPlaying) {
            nightVisionIsOn = !nightVisionIsOn;
            if (nightVisionIsOn) {
                nightVisionAudioSource.Play();
            }
        }

        if (nightVisionIsOn) {
            PostProcessingManager.Instance.SetNightVisionProfile();
        } else {
            PostProcessingManager.Instance.SetDefaultProfile();
        }
    }
}
