using Unity.Burst.CompilerServices;
using UnityEngine;

public class Paper : Interactable {
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;
    // inventory
    [Header("UI Prefab Reference")]
    public GameObject baseInventoryItemPrefab;

    [System.NonSerialized]
    public InventoryItem inventoryItem;

    private void Awake() {
        paperObj = transform.GetChild(0).gameObject;
    }

    public override void Collect() {
        PaperManager.Instance.CollectPaper(this);
    }
  
    public void SetInventoryItemData(int orderItem, string displayName) {
        if (inventoryItem == null) {
            inventoryItem = new(orderItem, displayName, InventoryItem.InteractableType.Item);
        } else {
            inventoryItem.defaultOrderItem = orderItem;
            inventoryItem.inventoryDisplayName = displayName;
            inventoryItem.interactableType = InventoryItem.InteractableType.Item;
            inventoryItem.displayOnInventory = true;
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
