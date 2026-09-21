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
    public bool canInvestigate;

    [System.NonSerialized]
    public Action<InventoryItem> equipActionToInvoke;

    public InventoryItem(int index, string displayName, InteractableType iType,
        bool equip = false, bool investigate = false,
        Action<InventoryItem> equipActionToInvoke = null) {
        defaultItemIndex = index;
        inventoryDisplayName = displayName;
        interactableType = iType;
        displayOnInventory = true;
        canEquip = equip;
        canInvestigate = investigate;
        this.equipActionToInvoke = equipActionToInvoke;
    }

    public int CompareTo(InventoryItem other) {
        if (other == null) return 1;
        return defaultItemIndex.CompareTo(other.defaultItemIndex);
    }
}
