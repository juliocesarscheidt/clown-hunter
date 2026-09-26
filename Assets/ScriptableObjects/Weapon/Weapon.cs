using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject {
    public string gunName;
    public GameObject gunPrefabObject;
    public GameObject particleShotEffect;
    public GameObject bulletHolePrefab;

    public Sprite gunReticleImage;

    public AudioClip gunShotSound;
    public AudioClip gunReloadSound;
    public AudioClip gunEmptySound;

    public Sprite gunIconImage;

    public float timeToShootInterval;
    public float timeToReloadInterval = 3f;

    public bool isAutomaticGun;

    public int currentBullets;
    public int maxBullets;
    public int availableBullets;

    public int hitDamage;
    public bool isEnabledByDefault;

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
            inventoryItem = new(index, displayName, InventoryItem.InteractableType.Gun,
                equip, inspect, usable, onEquipAction, onInstantiateAction);
        } else {
            inventoryItem.defaultItemIndex = index;
            inventoryItem.inventoryDisplayName = displayName;
            inventoryItem.interactableType = InventoryItem.InteractableType.Gun;
            inventoryItem.displayOnInventory = true;
            inventoryItem.canEquip = equip;
            inventoryItem.canInspect = inspect;
            inventoryItem.isUsable = usable;
            inventoryItem.onEquipAction = onEquipAction;
            inventoryItem.onInstantiateAction = onInstantiateAction;
        }
    }
}
