using UnityEngine;

public class GunAmmo : MonoBehaviour, Interactable
{
    private PlayerStats playerStats;
    public int bulletsAmount = 60;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    public void Collect() {
        HudManager.Instance.HidePressEObject();
        playerStats.CollectAmmo(bulletsAmount);
    }
}
