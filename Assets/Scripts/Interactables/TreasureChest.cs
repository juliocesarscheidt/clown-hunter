using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : Interactable {
    public override void OnInteract() {
        TreasureChestManager.Instance.StartTreasureChestInteraction();
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
