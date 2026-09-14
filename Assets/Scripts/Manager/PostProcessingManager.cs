using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }

    public Volume globalVolume;

    public VolumeProfile defaultProfile;
    public VolumeProfile nightVisionProfile;
    public VolumeProfile blackWhiteProfile;
    public VolumeProfile blackWhiteNightVisionProfile;
    private VolumeProfile previousProfile;

    public float defaultFog = 0.175f;
    public float nightVisionFog = 0.125f;
    private float previousFog;

    private bool nightVisionIsOn;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        previousProfile = defaultProfile;
        previousFog = defaultFog;
    }

    public void SetPreviousProfile() {
        globalVolume.profile = previousProfile;
        RenderSettings.fogDensity = previousFog;
    }

    public void SetBlackWhiteProfile() {
        // black and white is a temporary profile
        if (nightVisionIsOn) {
            globalVolume.profile = blackWhiteNightVisionProfile;
            RenderSettings.fogDensity = nightVisionFog;
        } else {
            globalVolume.profile = blackWhiteProfile;
            RenderSettings.fogDensity = defaultFog;
        }
    }

    public void SetNightVisionProfile() {
        previousProfile = nightVisionProfile;
        nightVisionIsOn = true;

        globalVolume.profile = nightVisionProfile;
        RenderSettings.fogDensity = nightVisionFog;
    }
  
    public void SetDefaultProfile() {
        previousProfile = defaultProfile;
        nightVisionIsOn = false;

        globalVolume.profile = defaultProfile;
        RenderSettings.fogDensity = defaultFog;
    }
}
    