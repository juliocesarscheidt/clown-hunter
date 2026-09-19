using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data Class
[System.Serializable]
public class InventoryItem {
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
}
