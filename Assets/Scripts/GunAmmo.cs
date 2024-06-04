using UnityEngine;

public class GunAmmo : MonoBehaviour, Interactable
{
    private GameObject player;
    private PlayerStats playerStats;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();
    }

    public void Collect() {
        HudManager.Instance.HidePressEObject();
        playerStats.CollectAmmo();
    }
}
