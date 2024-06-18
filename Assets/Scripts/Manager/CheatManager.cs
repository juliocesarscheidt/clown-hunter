using System.Collections.Generic;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    private PlayerStats playerStats;
    public float timeToType = 2f;
    private float typingTimer = 0.0f;
    private int currentTypingIndex = 0;
    public AudioSource cheatAudioSource;

    public enum Cheats {
        Invencible,
        InfiniteAmmo,
        InfiniteSprint,
    }

    public Dictionary<Cheats, string> cheatCodes = new() {
        {Cheats.Invencible, "SUPERHUMAN"},
        {Cheats.InfiniteAmmo, "AMMOGOD"},
        {Cheats.InfiniteSprint, "SPRINTGOD"},
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

            foreach (Cheats cheat in cheatCodes.Keys) {
                string cheatKeyCodeSequence = cheatCodes[cheat].ToString();
                int cheatKeyCodeLen = cheatKeyCodeSequence.Length;

                if (currentTypingIndex > cheatKeyCodeLen) {
                    continue;    
                }
                char nextExpectedKeyCode = cheatKeyCodeSequence[currentTypingIndex];

                if (currentKeyCode == nextExpectedKeyCode.ToString()) {
                    typingTimer = 0;
                    currentTypingIndex++;
                    if (currentTypingIndex == cheatKeyCodeLen) {
                        ActivateCheat(cheat);
                    }
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
        }
    }

    public void ActivateCheat(Cheats cheat) {
        HudManager.Instance.SetAndActivateCheatActivatedText($"Cheat activated {cheat}");
        cheatAudioSource.Play();

        switch (cheat) {
            case Cheats.Invencible:
                playerStats.takeDamage = false;
            break;

            case Cheats.InfiniteAmmo:
                playerStats.spendAmmo = false;
            break;

            case Cheats.InfiniteSprint:
                playerStats.SetSpendStamina(false);
            break;
        }
    }
}
