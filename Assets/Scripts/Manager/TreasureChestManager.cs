using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChestManager : MonoBehaviour
{
    public static TreasureChestManager Instance { get; private set; }
    private PlayerStats playerStats;
    public TreasureChest treasureChest;
    public GameObject padlockUIControllerPrefab;
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

        playerStats = FindObjectOfType<PlayerStats>();
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
            if (combinationNumbers[i] != otherCombinationNumbers[i]) {
                locked = true;
                return;
            }
        }

        locked = false;
        // TODO: play a sound
        // TODO: HideOverlayCanvas
        // TODO: give a key to the player, add it to inventory
        // TODO: hide the treasure chest gameObject
        HudManager.Instance.HideOverlayCanvas();
    }

    void Update() {
        if (!GlobalGameplayManager.Instance.IsGameplayActiveForOverlayCanvas) {
            return;
        }
    }

    public void StartInteraction() {
        if (!locked) {
            Debug.Log("Treasure chest is unlocked!");
            return;
        }
        HudManager.Instance.ShowOverlayCanvas();
        HudManager.Instance.SpawnItemOnOverlayCanvasSlot(padlockUIControllerPrefab);
    }

    public bool IsLocked {
        get { return locked; }
    }
}
