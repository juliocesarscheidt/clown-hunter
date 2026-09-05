using UnityEngine;

public class FirstAid : Interactable {
    public int healthAmount = 100;

    public override void Collect() {
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
