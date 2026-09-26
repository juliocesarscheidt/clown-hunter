using System;
using UnityEngine;

public abstract class InteractableInventory : Interactable {
    // inventory
    [Header("UI Prefab Reference")]
    public GameObject baseInventoryItemPrefab;

    [System.NonSerialized]
    public InventoryItem inventoryItem;

    public void SetInventoryItemData(int index, string displayName,
        bool equip = false, bool inspect = false, bool usable = false,
        Action<InventoryItem> onEquipAction = null,
        Action<InventoryItem, GameObject> onInstantiateAction = null) {
        if (inventoryItem == null) {
            inventoryItem = new(index, displayName, InventoryItem.InteractableType.Item,
                equip, inspect, usable, onEquipAction, onInstantiateAction);
        } else {
            inventoryItem.defaultItemIndex = index;
            inventoryItem.inventoryDisplayName = displayName;
            inventoryItem.interactableType = InventoryItem.InteractableType.Item;
            inventoryItem.displayOnInventory = true;
            inventoryItem.canEquip = equip;
            inventoryItem.canInspect = inspect;
            inventoryItem.isUsable = usable;
            inventoryItem.onEquipAction = onEquipAction;
            inventoryItem.onInstantiateAction = onInstantiateAction;
        }
    }
}
