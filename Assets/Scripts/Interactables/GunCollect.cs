using UnityEngine;

public class GunCollect : Interactable {
    public string gunName; // the same as the scriptable weapon name
    [SerializeField]
    private int playerGunIndex;

    public new void Start() {
        base.Start();
        playerGunIndex = playerStats.guns.FindIndex((w) => w.gunName == gunName);
    }

    public override void Collect() {
        // Debug.Log($"playerGunIndex {playerGunIndex}");
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
