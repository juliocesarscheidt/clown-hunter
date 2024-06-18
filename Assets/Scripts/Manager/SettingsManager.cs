using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField]
    private AudioSource[] audioSources;
    private float[] audioSourcesOriginalVolumes;

    public Slider audioSlider;
    public float defaultSoundVolume = 1f;
    public TextMeshProUGUI volumeInfoText;

    public TMP_Dropdown difficultyDropdown;
    public int defaultDifficulty = 0;
    public int enemiesToAddOnPaperCollectedByDifficulty = 2;

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
        float volume = PlayerPrefs.GetFloat("volume", defaultSoundVolume);
        audioSlider.value = volume;
        SetSoundSettings(volume);

        int difficulty = PlayerPrefs.GetInt("difficulty", defaultDifficulty);
        difficultyDropdown.value = difficulty;
        SetDifficultySettings(difficulty);
    }

    private void SetDifficultySettings(int difficulty) {
        // increases the amount of spawned enemies on each collected paper
        // difficulty 0 = + 2 enemies
        // difficulty 1 = + 4 enemies
        // difficulty 2 = + 6 enemies
        if (PaperManager.Instance != null) {
            PaperManager.Instance.enemiesToAddOnPaperCollected = (difficulty + 1) * enemiesToAddOnPaperCollectedByDifficulty;
        }

        // difficulty 0 = regularHitDamage
        // difficulty 1 = regularHitDamage - 5
        // difficulty 2 = regularHitDamage - 10
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null) {
            playerStats.regularHitDamage = (difficulty * -5) + playerStats.defaultRegularHitDamage;
        }

        // difficulty 0 = regularHitDamage
        // difficulty 1 = regularHitDamage + 10
        // difficulty 2 = regularHitDamage + 20
        if (MonsterManager.Instance != null) {
            MonsterManager.Instance.regularHitDamage = (difficulty * 10) + MonsterManager.Instance.defaultRegularHitDamage;
        }
    }

    public void ApplyDifficultySettings() {
        int difficulty = difficultyDropdown.value;
        PlayerPrefs.SetInt("difficulty", difficulty);
        SetDifficultySettings(difficulty);
        if (MonsterManager.Instance != null) {
            MonsterManager.Instance.SpawnEnemiesDelayed();
        }
    }

    private void SetSoundSettings(float soundVolume) {
        for (int i = 0; i < audioSources.Length; i++) {
            float volumeOriginal = audioSourcesOriginalVolumes[i];
            audioSources[i].volume = volumeOriginal * soundVolume;
        }
    }

    public void ApplySoundSettings() {
        float volume = audioSlider.value;
        PlayerPrefs.SetFloat("volume", volume);
        SetSoundSettings(volume);
    }

    public void SetVolumeInfoText() {
        volumeInfoText.text = $"{Mathf.Round(audioSlider.value * 100)}";
    }
}
