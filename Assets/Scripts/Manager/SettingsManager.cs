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
    public TextMeshProUGUI volumeInfoText;
    public float defaultVolume = 1f;
    public float volume;

    public TMP_Dropdown difficultyDropdown;
    public int defaultDifficulty = 0;
    public int maxDifficulty = 3;
    public int enemiesToAddOnPaperCollectedByDifficulty = 2;
    public int difficulty;

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
        volume = PlayerPrefs.GetFloat("volume", defaultVolume);
        audioSlider.value = volume;
        SetSoundSettings(volume);

        difficulty = PlayerPrefs.GetInt("difficulty", defaultDifficulty);
        difficultyDropdown.value = difficulty;
        SetDifficultySettings(difficulty);
    }

    private void SetDifficultySettings(int difficulty) {
        if (difficulty < 0 || difficulty > maxDifficulty) return;
        /*
         * 0 = Easy
         * 1 = Normal
         * 2 = Gard
         * 3 = God (no cheats allowed)
         */
        if (difficulty == maxDifficulty) {
            CheatManager.Instance.DeactivateCheats();
        }

        // increases the amount of spawned enemies on each collected paper
        // difficulty 0 = + 2 enemies
        // difficulty 1 = + 4 enemies
        // difficulty 2 = + 6 enemies
        // difficulty 3 = + 8 enemies
        if (PaperManager.Instance != null) {
            PaperManager.Instance.enemiesToAddOnPaperCollected = (difficulty + 1) * enemiesToAddOnPaperCollectedByDifficulty;
        }

        // difficulty 0 = regularHitDamage
        // difficulty 1 = regularHitDamage - 5
        // difficulty 2 = regularHitDamage - 10
        // difficulty 3 = regularHitDamage - 15
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null) {
            playerStats.regularHitDamage = (difficulty * -5) + playerStats.defaultRegularHitDamage;
        }

        // difficulty 0 = regularHitDamage
        // difficulty 1 = regularHitDamage + 10
        // difficulty 2 = regularHitDamage + 20
        // difficulty 3 = regularHitDamage + 30
        if (MonsterManager.Instance != null) {
            MonsterManager.Instance.regularHitDamage = (difficulty * 10) + MonsterManager.Instance.defaultRegularHitDamage;
        }

        // difficulty 0 = maxSimultaneousAttacks = 2
        // difficulty 1 = maxSimultaneousAttacks = 2
        // difficulty 2 = maxSimultaneousAttacks = 3
        // difficulty 3 = maxSimultaneousAttacks = 3
        if (MonsterManager.Instance != null) {
            MonsterManager.Instance.maxSimultaneousAttacks = Mathf.CeilToInt((difficulty / 2f) + 1.5f);
        }
    }

    public void ApplyDifficultySettings() {
        difficulty = difficultyDropdown.value;
        PlayerPrefs.SetInt("difficulty", difficulty);
        SetDifficultySettings(difficulty);
        if (MonsterManager.Instance != null) {
            MonsterManager.Instance.SpawnEnemiesDelayed();
        }
    }

    private void SetSoundSettings(float volume) {
        for (int i = 0; i < audioSources.Length; i++) {
            float originalVolume = audioSourcesOriginalVolumes[i];
            audioSources[i].volume = originalVolume * volume;
        }
    }

    public void ApplySoundSettings() {
        volume = audioSlider.value;
        PlayerPrefs.SetFloat("volume", volume);
        SetSoundSettings(volume);
    }

    public void SetVolumeInfoText() {
        volumeInfoText.text = $"{Mathf.Round(audioSlider.value * 100)}";
    }
}
