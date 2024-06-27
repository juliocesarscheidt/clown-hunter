using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }

    public Volume globalVolume;
    public VolumeProfile defaultProfile;
    public VolumeProfile nightVisionProfile;

    public float defaultFog = 0.175f;
    public float nightVisionFog = 0.125f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    public void SetNightVisionProfile() {
        globalVolume.profile = nightVisionProfile;
        RenderSettings.fogDensity = nightVisionFog;
    }

    public void SetDefaultProfile() {
        globalVolume.profile = defaultProfile;
        RenderSettings.fogDensity = defaultFog;
    }
}
    