using UnityEngine;

public class GunAmmo : MonoBehaviour, Interactable
{
    private PlayerStats playerStats;
    public int bulletsAmount;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    public void Collect() {
        HudManager.Instance.HidePressEObject();
        playerStats.CollectAmmo(bulletsAmount);
    }
}
