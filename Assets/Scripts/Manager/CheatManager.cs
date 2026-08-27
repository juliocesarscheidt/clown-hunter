using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance { get; private set; }

    private PlayerStats playerStats;
    public AudioSource cheatAudioSource;
    public float timeToType = 2f;

    [SerializeField]
    private string currentInput = string.Empty;

    [SerializeField]
    private float resetTimeout = 3.0f; // Reset if user stops typing

    private float lastKeyTime;
    private int maxCheatLength;

    public enum CheatEnum {
        INFINITE_AMMO,
        INFINITE_SPRINT,
        INVENCIBLE_PLAYER,
        INVENCIBLE_MONSTERS,
        MAD_MONSTERS,
        SHOW_PAPERS,
        DEVMODE,
    }

    public Dictionary<string, CheatEnum> cheatCodes = new() {
        {"AMMOGOD", CheatEnum.INFINITE_AMMO},
        {"RUNNER", CheatEnum.INFINITE_SPRINT},
        {"SUPERHUMAN", CheatEnum.INVENCIBLE_PLAYER},
        {"OMNIMONSTERS", CheatEnum.INVENCIBLE_MONSTERS},
        {"MADMONSTERS", CheatEnum.MAD_MONSTERS},
        {"SHOWPAPERS", CheatEnum.SHOW_PAPERS},
        {"DEVMODE", CheatEnum.DEVMODE},
    };

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerStats = FindObjectOfType<PlayerStats>();

        // Cache the longest cheat length to cap the buffer size
        maxCheatLength = cheatCodes.Keys.Max(c => c.Length);
    }

    private void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead || playerStats.isReloading) {
            return;
        }
        if (currentInput.Length > 0 && Time.time - lastKeyTime > resetTimeout) {
            currentInput = string.Empty;
        }

        if (!string.IsNullOrEmpty(Input.inputString)){
            foreach (char c in Input.inputString)  {
                if (c == "\b"[0] || c == "\n"[0] || c == "\r"[0]) {
                    currentInput = string.Empty;
                    continue;
                }
                currentInput += c;
                lastKeyTime = Time.time;

                if (currentInput.Length > maxCheatLength) {
                    currentInput = currentInput.Substring(currentInput.Length - maxCheatLength);
                }

                CheckCheatCodes();
            }
        }
    }

    private void CheckCheatCodes()    {
        foreach (var pair in cheatCodes) {
            if (currentInput.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase)) {
                ActivateCheat(pair.Value);
                currentInput = string.Empty; // Clear buffer after activation
                break;
            }
        }
    }

    public void DeactivateCheats() {
        foreach (CheatEnum cheat in cheatCodes.Values) {
            DeactivateCheat(cheat);
        }
        HudManager.Instance.SetAndActivateCheatActivatedText("Cheats deactivated");
    }

    public void DeactivateCheat(CheatEnum cheat) {
        switch (cheat) {
            case CheatEnum.INFINITE_AMMO:
                playerStats.spendAmmo = true;
                break;
            case CheatEnum.INFINITE_SPRINT:
                playerStats.SetSpendStamina(true);
                break;
            case CheatEnum.INVENCIBLE_PLAYER:
                playerStats.canReceiveDamage = true;
                break;
            case CheatEnum.INVENCIBLE_MONSTERS:
                MonsterManager.Instance.ChangeCanReceiveDamageToAllMonsters(true);
                break;
            case CheatEnum.MAD_MONSTERS:
                MonsterManager.Instance.ResetDefaultRunProbabilityPercentageToAllMonsters();
                break;
            case CheatEnum.SHOW_PAPERS:
                PaperManager.Instance.ShowAllPapers(false);
                break;
            case CheatEnum.DEVMODE:
                HudManager.Instance.showFps = false;
                MonsterManager.Instance.ChangeShowCurrentStateToAllMonsters(false);
            break;
        }
    }

    public void ActivateCheat(CheatEnum cheat) {
        if (SettingsManager.Instance.GetDifficulty() == SettingsManager.Instance.maxDifficulty) {
            HudManager.Instance.SetAndActivateCheatActivatedText("No cheats allowed");
            return;
        }

        HudManager.Instance.SetAndActivateCheatActivatedText("Cheat activated");
        cheatAudioSource.Play();

        switch (cheat) {
            case CheatEnum.INFINITE_AMMO:
                playerStats.spendAmmo = false;
                playerStats.FillAllAmmo();
                break;
            case CheatEnum.INFINITE_SPRINT:
                playerStats.SetSpendStamina(false);
                break;
            case CheatEnum.INVENCIBLE_PLAYER:
                playerStats.canReceiveDamage = false;
                playerStats.FillHealth();
            break;
            case CheatEnum.INVENCIBLE_MONSTERS:
                MonsterManager.Instance.ChangeCanReceiveDamageToAllMonsters(false);
            break;
            case CheatEnum.MAD_MONSTERS:
                MonsterManager.Instance.ChangeRunProbabilityPercentageToAllMonsters(100f);
                break;
            case CheatEnum.SHOW_PAPERS:
                PaperManager.Instance.ShowAllPapers(true);
                break;
            case CheatEnum.DEVMODE:
                HudManager.Instance.showFps = true;
                MonsterManager.Instance.ChangeShowCurrentStateToAllMonsters(true);
            break;
        }
    }
}
