using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class NightVisionManager : MonoBehaviour
{
    private PlayerStats playerStats;
    public Volume globalVolume;
    public VolumeProfile defaultProfile;
    public VolumeProfile nightVisionProfile;
    public AudioSource nightVisionAudioSource;
    private bool nightVisionIsOn = false;
    public float defaultFog = 0.175f;
    public float nightVisionFog = 0.125f;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.N) && !nightVisionAudioSource.isPlaying) {
            nightVisionIsOn = !nightVisionIsOn;
            if (nightVisionIsOn) {
                nightVisionAudioSource.Play();
            }
        }

        if (nightVisionIsOn) {
            globalVolume.profile = nightVisionProfile;
            RenderSettings.fogDensity = nightVisionFog;
        } else {
            globalVolume.profile = defaultProfile;
            RenderSettings.fogDensity = defaultFog;
        }
    }
}
