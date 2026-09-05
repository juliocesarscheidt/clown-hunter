using UnityEngine;

public class GunAmmo : Interactable {
    public int bulletsAmount = 60;
    
    public override void Collect() {
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
