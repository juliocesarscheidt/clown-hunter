using System;
using UnityEngine;

public abstract class InteractableInventory : Interactable {
    // inventory
    [Header("UI Prefab Reference")]
    public GameObject baseInventoryItemPrefab;

    [System.NonSerialized]
    public InventoryItem inventoryItem;

    public void SetInventoryItemData(int index, string displayName,
        bool equip = false, bool investigate = false,
        Action<InventoryItem> equipActionToInvoke = null, Action<InventoryItem> investigateActionToInvoke = null) {
        if (inventoryItem == null) {
            inventoryItem = new(index, displayName, InventoryItem.InteractableType.Item,
                equip, investigate, equipActionToInvoke, investigateActionToInvoke);
        } else {
            inventoryItem.defaultItemIndex = index;
            inventoryItem.inventoryDisplayName = displayName;
            inventoryItem.interactableType = InventoryItem.InteractableType.Item;
            inventoryItem.displayOnInventory = true;
            inventoryItem.canEquip = equip;
            inventoryItem.canInvestigate = investigate;
            inventoryItem.equipActionToInvoke = equipActionToInvoke;
            inventoryItem.investigateActionToInvoke = investigateActionToInvoke;
        }
    }
}
