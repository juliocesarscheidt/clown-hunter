using System;

public class InventoryItemDynamicData {
    public int index;
}

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

    [System.NonSerialized]
    public Action<InventoryItem> onEquipAction;

    public InventoryItem(int index, string displayName, InteractableType iType,
        bool equip = false, bool inspect = false,
        Action<InventoryItem> onEquipAction = null) {
        defaultItemIndex = index;
        inventoryDisplayName = displayName;
        interactableType = iType;
        displayOnInventory = true;
        canEquip = equip;
        canInspect = inspect;
        this.onEquipAction = onEquipAction;
    }

    public int CompareTo(InventoryItem other) {
        if (other == null) return 1;
        return defaultItemIndex.CompareTo(other.defaultItemIndex);
    }
}
