using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChestManager : MonoBehaviour
{
    public static TreasureChestManager Instance { get; private set; }
    public GameObject treasureChestObj;
    public GameObject padlockUIControllerPrefab;
    public GameObject keyInventoryItemPrefab;
    [SerializeField]
    private bool locked = true;
    [SerializeField]
    private List<int> combinationNumbers = new();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    public void Start() {
        ToggleTreasureChest(false);
    }

    public void AddCombinationNumber(int number) {
        combinationNumbers.Add(number);
    }

    public void CompareCombinationNumbers(List<int> otherCombinationNumbers) {
        if (combinationNumbers.Count != otherCombinationNumbers.Count) {
            locked = true;
            return;
        }

        for (int i = 0; i < combinationNumbers.Count; i++) {
            // not match
            if (combinationNumbers[i] != otherCombinationNumbers[i]) {
                locked = true;
                return;
            }
        }

        locked = false;
        Unlock();
    }

    public void Unlock() {
        // TODO: play a sound

        // give a key to the player, adding it to the inventory
        var currentIndex = InventoryManager.Instance.InteractableItemsCount;
        InventoryItem keyInventory = new(currentIndex, "Secret Key", InventoryItem.InteractableType.Item,
            false, true, true, null, null);
        InventoryManager.Instance.AddInteractableItem(keyInventory, keyInventoryItemPrefab);

        // show the canvas with the key
        HudManager.Instance.SpawnItemOnOverlayCanvasSlot(keyInventoryItemPrefab);

        // hide the treasure chest gameObject
        ToggleTreasureChest(false);
    }

    public void StartTreasureChestInteraction() {
        if (!locked) {
            Debug.Log("Treasure chest is unlocked!");
            return;
        }
        HudManager.Instance.ShowOverlayCanvas();
        HudManager.Instance.SpawnItemOnOverlayCanvasSlot(padlockUIControllerPrefab);
    }

    public void ToggleTreasureChest(bool enabled) {
        treasureChestObj.SetActive(enabled);
    }

    public bool IsLocked {
        get { return locked; }
    }
}
