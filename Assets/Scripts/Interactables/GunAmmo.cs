using UnityEngine;

public class GunAmmo : Interactable
{
    private PlayerStats playerStats;
    public int bulletsAmount = 60;
    private Outline outlineScript;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();
    }

    public override void Collect() {
        HudManager.Instance.HidePressEObject();
        playerStats.CollectAmmo(bulletsAmount);
    }

    public override void EnableOutline() {
        outlineScript.enabled = true;
    }

    public override void DisableOutline() {
        outlineScript.enabled = false;
    }
}
