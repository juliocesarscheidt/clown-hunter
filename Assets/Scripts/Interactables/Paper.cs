using UnityEngine;

public class Paper : Interactable {
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;
    public InventoryItem inventoryItem;

    private void Awake() {
        paperObj = transform.GetChild(0).gameObject;
        inventoryItem = GetComponent<InventoryItem>();
    }

    public override void Collect() {
        PaperManager.Instance.CollectPaper(this);
    }

    public void SetInventoryItemData(int defaultOrderItem, string inventoryDisplayName) {
        if (inventoryItem != null) {
            inventoryItem.DefaultOrderItem = defaultOrderItem;
            inventoryItem.InventoryDisplayName = inventoryDisplayName;
        }
    }

    public void SetPaperObjLayer(int layer) {
        if (paperObj != null) {
            paperObj.layer = layer;
        }
    }

    public void SetForcedOutlineEnabled() {
        forcedOutlineEnabled = true;
        EnableOutline();
    }

    public void SetForcedOutlineDisabled() {
        forcedOutlineEnabled = false;
        DisableOutline();
    }

    public override void EnableOutline() {
        isOutlineEnabled = true;
        outlineScript.enabled = isOutlineEnabled;
    }

    public override void DisableOutline() {
        if (!forcedOutlineEnabled) {
            isOutlineEnabled = false;
            outlineScript.enabled = isOutlineEnabled;
        }
    }
}
