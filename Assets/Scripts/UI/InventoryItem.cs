using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data Class
[System.Serializable]
public class InventoryItem : MonoBehaviour {
    // these will be set dynamically
    private int defaultOrderItem;
    private string inventoryDisplayName;

    public enum InteractableType {
        Gun = 0,
        Item = 1,
    }
    public InteractableType interactableType;
    public bool displayOnInventory;
    public GameObject baseInventoryItemPrefab;

    public int DefaultOrderItem {
        get => defaultOrderItem;
        set => defaultOrderItem = value;
    }

    public string InventoryDisplayName {
        get => inventoryDisplayName;
        set => inventoryDisplayName = value;
    }
}
