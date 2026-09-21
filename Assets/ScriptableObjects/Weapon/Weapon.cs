using System;
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
        bool equip = false, bool investigate = false,
        Action<InventoryItem> equipActionToInvoke = null) {
        if (inventoryItem == null) {
            inventoryItem = new(index, displayName, InventoryItem.InteractableType.Gun,
                equip, investigate, equipActionToInvoke);
        } else {
            inventoryItem.defaultItemIndex = index;
            inventoryItem.inventoryDisplayName = displayName;
            inventoryItem.interactableType = InventoryItem.InteractableType.Gun;
            inventoryItem.displayOnInventory = true;
            inventoryItem.canEquip = equip;
            inventoryItem.canInvestigate = investigate;
            inventoryItem.equipActionToInvoke = equipActionToInvoke;
        }
    }
}
