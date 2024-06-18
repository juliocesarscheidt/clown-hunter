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
        HudManager.Instance.HidePressEObject();
        playerStats.CollectFirstAid(healthAmount);
    }

    public override void EnableOutline() {
        outlineScript.enabled = true;
    }

    public override void DisableOutline() {
        outlineScript.enabled = false;
    }
}
