using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    private PlayerStats playerStats;
    public AudioSource cheatAudioSource;

    public float timeToType = 2f;
    private float typingTimer = 0.0f;
    
    [SerializeField]
    private int currentTypingIndex = 0;
    [SerializeField]
    private string typedString = string.Empty;

    public enum CheatEnum {
        Invencible,
        InfiniteAmmo,
        InfiniteSprint,
    }

    public Dictionary<string, CheatEnum> cheatCodes = new() {
        {"AMMOGOD", CheatEnum.InfiniteAmmo},
        {"SPRINTGOD", CheatEnum.InfiniteSprint},
        {"SUPERHUMAN", CheatEnum.Invencible},
    };

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void OnGUI() {
        Event e = Event.current;

        if (
            e.type == EventType.KeyDown &&
            e.keyCode.ToString().Length == 1 &&
            char.IsLetter(e.keyCode.ToString()[0])
        ){
            string currentKeyCode = e.keyCode.ToString();

            List<string> cheatKeysToCheck = cheatCodes.Keys.ToList().FindAll(cheat => cheat.StartsWith(typedString));

            foreach (string cheatSequence in cheatKeysToCheck) {
                CheatEnum cheat = cheatCodes[cheatSequence];
                int cheatKeyCodeLen = cheatSequence.Length;

                if (currentTypingIndex > cheatKeyCodeLen) {
                    continue;    
                }

                char expectedKeyCode = cheatSequence[currentTypingIndex];

                if (currentKeyCode == expectedKeyCode.ToString()) {
                    typingTimer = 0;
                    currentTypingIndex++;
                    typedString = $"{typedString}{expectedKeyCode}";

                    if (currentTypingIndex == cheatKeyCodeLen) {
                        currentTypingIndex = 0;
                        typedString = string.Empty;
                        ActivateCheat(cheat);
                    }

                    break;
                }
            }
        }
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead || playerStats.isReloading) {
            return;
        }
        typingTimer += Time.deltaTime;
        if (typingTimer >= timeToType) {
            currentTypingIndex = 0;
            typedString = string.Empty;
        }
    }

    public void ActivateCheat(CheatEnum cheat) {
        HudManager.Instance.SetAndActivateCheatActivatedText($"Cheat activated {cheat}");
        cheatAudioSource.Play();

        switch (cheat) {
            case CheatEnum.InfiniteAmmo:
                playerStats.spendAmmo = false;
                playerStats.FillAllAmmo();
                break;

            case CheatEnum.InfiniteSprint:
                playerStats.SetSpendStamina(false);
                break;

            case CheatEnum.Invencible:
                playerStats.takeDamage = false;
                playerStats.FillHealth();
            break;
        }
    }
}
