using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }

    [SerializeField]
    private AudioSource[] audioSources;
    private float[] audioSourcesOriginalVolumes;

    public float defaultSoundVolume = 1f;
    public Slider audioSlider;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        audioSourcesOriginalVolumes = new float[audioSources.Length];
        for (int i = 0; i < audioSources.Length; i++) {
            audioSourcesOriginalVolumes[i] = audioSources[i].volume;
        }
    }

    private void Start() {
        float soundVolume = PlayerPrefs.GetFloat("sound_volume", defaultSoundVolume);
        audioSlider.value = soundVolume;
        SetSoundSettings(soundVolume);
    }

    private void SetSoundSettings(float soundVolume) {
        for (int i = 0; i < audioSources.Length; i++) {
            float volumeOriginal = audioSourcesOriginalVolumes[i];
            audioSources[i].volume = volumeOriginal * soundVolume;
        }
    }

    public void ApplySoundSettings() {
        float soundVolume = audioSlider.value;
        PlayerPrefs.SetFloat("sound_volume", soundVolume);
        SetSoundSettings(soundVolume);
    }
}
