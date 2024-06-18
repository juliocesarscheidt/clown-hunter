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
        isOutlineEnabled = true;
        outlineScript.enabled = isOutlineEnabled;
    }

    public override void DisableOutline() {
        isOutlineEnabled = false;
        outlineScript.enabled = isOutlineEnabled;
    }
}
