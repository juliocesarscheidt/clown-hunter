using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }

    public Volume globalVolumeGeneral;

    public VolumeProfile defaultProfile;
    public VolumeProfile nightVisionProfile;
    public VolumeProfile blackWhiteProfile;
    public VolumeProfile blackWhiteNightVisionProfile;
    private VolumeProfile previousProfile;

    public Volume globalVolumeBlur;

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
        DisableBlur();
    }

    public void EnableBlur()  {
        globalVolumeBlur.enabled = true;
    }

    public void DisableBlur() {
        globalVolumeBlur.enabled = false;
    }

    public void SetPreviousProfile() {
        globalVolumeGeneral.profile = previousProfile;
        RenderSettings.fogDensity = previousFog;
    }

    public void SetBlackWhiteProfile() {
        // black and white is a temporary profile
        if (nightVisionIsOn) {
            globalVolumeGeneral.profile = blackWhiteNightVisionProfile;
            RenderSettings.fogDensity = nightVisionFog;
        } else {
            globalVolumeGeneral.profile = blackWhiteProfile;
            RenderSettings.fogDensity = defaultFog;
        }
    }

    public void SetNightVisionProfile() {
        previousProfile = nightVisionProfile;
        nightVisionIsOn = true;

        globalVolumeGeneral.profile = nightVisionProfile;
        RenderSettings.fogDensity = nightVisionFog;
    }
  
    public void SetDefaultProfile() {
        previousProfile = defaultProfile;
        nightVisionIsOn = false;

        globalVolumeGeneral.profile = defaultProfile;
        RenderSettings.fogDensity = defaultFog;
    }
}
    