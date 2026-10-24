using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CutsceneManager;

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
    // inventory global id for the key item that will be added to the inventory when the treasure chest is unlocked
    public string KeyInventoryItemID = "item_secret_key_1";

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
        // give a key to the player, adding it to the inventory
        var currentIndex = InventoryManager.Instance.InteractableItemsCount;
        InventoryItem keyInventory = new(currentIndex, "Secret Key", InventoryItem.InteractableType.Item,
            false, true, true, null, null);
        InventoryManager.Instance.AddInteractableItem(keyInventory, keyInventoryItemPrefab);
        keyInventory.SetGlobalInventoryItemID(KeyInventoryItemID);

        // when closing the canvas, play a cutscene
        void onCloseAction() {
            CutsceneManager.Instance.PlayCutscene(CutsceneType.ShowSelectedDoorCutscene, 3.0f);
        };
        // show the canvas with the key
        var message = $"{keyInventory.inventoryDisplayName} was added to inventory";
        HudManager.Instance.SpawnItemOnOverlayCanvasSlot(keyInventoryItemPrefab, message, onCloseAction);

        // hide the treasure chest gameObject
        ToggleTreasureChest(false);
    }

    public void StartTreasureChestInteraction() {
        if (!locked) {
            Debug.Log("Treasure chest is unlocked!");
            return;
        }
        HudManager.Instance.HUDShowOverlayCanvas();
        HudManager.Instance.SpawnItemOnOverlayCanvasSlot(padlockUIControllerPrefab);
    }

    public void ToggleTreasureChest(bool enabled) {
        treasureChestObj.SetActive(enabled);
    }

    public bool IsLocked {
        get { return locked; }
    }
}
