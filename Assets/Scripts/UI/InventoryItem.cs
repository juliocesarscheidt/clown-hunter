using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data Class
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
    
    public InventoryItem() {}

    // enabled by default
    public InventoryItem(int orderItem, string displayName, InteractableType iType) {
        defaultOrderItem = orderItem;
        inventoryDisplayName = displayName;
        interactableType = iType;
        displayOnInventory = true;
    }

    public int CompareTo(InventoryItem other) {
        if (other == null) return 1;
        int result = defaultOrderItem.CompareTo(other.defaultOrderItem);
        if (result == 0 && inventoryDisplayName != null && other.inventoryDisplayName != null) {
            return inventoryDisplayName.CompareTo(other.inventoryDisplayName);
        }
        return result;
    }
}
