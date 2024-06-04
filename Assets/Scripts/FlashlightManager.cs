using UnityEngine;

public class FlashlightManager : MonoBehaviour
{
    private GameObject player;
    private PlayerStats playerStats;

    public Light flashlight;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();

        if (flashlight != null) {
            flashlight.enabled = false;
        }
    }

    void Update() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
