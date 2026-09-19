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
    [HideInInspector]
    private InventoryItem inventoryItem;

    public void SetInventoryItemData(int orderItem, string displayName) {
        if (inventoryItem == null) {
            inventoryItem = new(orderItem, displayName, InventoryItem.InteractableType.Gun);
        } else {
            inventoryItem.defaultOrderItem = orderItem;
            inventoryItem.inventoryDisplayName = displayName;
        }
    }

    public InventoryItem GetInventoryItem() {
        return inventoryItem;
    }
}
