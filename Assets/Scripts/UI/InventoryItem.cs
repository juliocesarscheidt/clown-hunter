using System;
using UnityEngine;

[System.Serializable]
public class InventoryItem : IComparable<InventoryItem> {
    public int defaultItemIndex;
    public string inventoryDisplayName;
    public enum InteractableType {
        Gun = 0,
        Item = 1,
    }
    public InteractableType interactableType;
    public bool displayOnInventory;

    public bool canEquip;
    public bool canInspect;
    public bool isUsable;

    private string globalInventoryItemID;

    [System.NonSerialized]
    public Action<InventoryItem> onEquipAction;

    [System.NonSerialized]
    public Action<InventoryItem, GameObject> onInstantiateAction;

    public InventoryItem(int index, string displayName, InteractableType iType,
        bool equip = false, bool inspect = false, bool usable = false,
        Action<InventoryItem> onEquipAction = null,
        Action<InventoryItem, GameObject> onInstantiateAction = null) {
        defaultItemIndex = index;
        inventoryDisplayName = displayName;
        interactableType = iType;
        displayOnInventory = true;
        canEquip = equip;
        canInspect = inspect;
        isUsable = usable;
        this.onEquipAction = onEquipAction;
        this.onInstantiateAction = onInstantiateAction;

        if (isUsable) {
            SetGlobalInventoryItemID();
        }
    }

    public void SetGlobalInventoryItemID(string keyID = "") {
        if (keyID == "") {
            globalInventoryItemID = $"{interactableType}_{inventoryDisplayName}_{defaultItemIndex}".ToLower();
        } else {
            globalInventoryItemID = keyID;
        }
        Debug.Log($"SetGlobalInventoryItemID globalInventoryItemID {globalInventoryItemID}");
    }

    public string GlobalInventoryItemID {
        get { return globalInventoryItemID; }
    }

    public int CompareTo(InventoryItem other) {
        if (other == null) return 1;
        return defaultItemIndex.CompareTo(other.defaultItemIndex);
    }
}
