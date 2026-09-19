using System;

[System.Serializable]
public class InventoryItem : IComparable<InventoryItem> {
    public int defaultOrderItem;
    public string inventoryDisplayName;
    public enum InteractableType {
        Gun = 0,
        Item = 1,
    }
    public InteractableType interactableType;
    public bool displayOnInventory;

    // enabled by default
    public InventoryItem(int orderItem, string displayName, InteractableType iType) {
        defaultOrderItem = orderItem;
        inventoryDisplayName = displayName;
        interactableType = iType;
        displayOnInventory = true;
    }

    public int CompareTo(InventoryItem other) {
        if (other == null) return 1;
        return defaultOrderItem.CompareTo(other.defaultOrderItem);
    }
}
