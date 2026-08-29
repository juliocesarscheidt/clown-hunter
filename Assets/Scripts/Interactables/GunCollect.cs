using UnityEngine;

public class GunCollect : Interactable
{
    private PlayerStats playerStats;
    private Outline outlineScript;

    public string gunName; // the same as the scriptable weapon name
    private int playerGunIndex;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();

        playerGunIndex = playerStats.guns.FindIndex((w) => w.gunName == gunName);
    }

    public override void Collect() {
        HudManager.Instance.HidePressInteractObject();
        Debug.Log($"playerGunIndex {playerGunIndex}");

        playerStats.CollectGunSetEnabled(playerGunIndex);
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
