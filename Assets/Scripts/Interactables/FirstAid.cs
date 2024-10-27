using UnityEngine;

public class FirstAid : Interactable
{
    private PlayerStats playerStats;
    public int healthAmount = 100;
    private Outline outlineScript;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();
    }

    public override void Collect() {
        HudManager.Instance.HidePressInteractObject();
        playerStats.CollectFirstAid(healthAmount);
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
