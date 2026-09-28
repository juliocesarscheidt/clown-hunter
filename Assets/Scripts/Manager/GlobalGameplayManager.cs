using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameplayManager : MonoBehaviour
{
    public static GlobalGameplayManager Instance { get; private set; }
    private PlayerStats playerStats;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerStats = FindObjectOfType<PlayerStats>();
    }
  
    public bool IsGameplayActive {
        get {
            return HudManager.Instance.IsRunningGame &&
                !HudManager.Instance.IsPaused &&
                !HudManager.Instance.IsShowingOverlayCanvas &&
                !playerStats.isDead &&
                !InventoryManager.Instance.IsShowingInventory;
            // not checking for playerStats.isReloading
        }
    }

    public bool IsGameplayActiveNotReloading {
        get {
            return HudManager.Instance.IsRunningGame &&
                !HudManager.Instance.IsPaused &&
                !HudManager.Instance.IsShowingOverlayCanvas &&
                !playerStats.isDead &&
                !InventoryManager.Instance.IsShowingInventory &&
                !playerStats.isReloading;
        }
    }

    public bool IsGameplayActiveForInventory {
        get {
            return HudManager.Instance.IsRunningGame && 
                !HudManager.Instance.IsPaused &&
                !HudManager.Instance.IsShowingOverlayCanvas &&
                !playerStats.isDead;
            // not checking for InventoryManager.Instance.IsShowingInventory
        }
    }

    public bool IsGameplayActiveForOverlayCanvas {
        get {
            return HudManager.Instance.IsRunningGame &&
                !HudManager.Instance.IsPaused &&
                !playerStats.isDead &&
                !InventoryManager.Instance.IsShowingInventory;
            // not checking for HudManager.Instance.IsShowingOverlayCanvas
        }
    }

    public bool IsGameplayPaused {
        get {
            return HudManager.Instance.IsPaused ||
                HudManager.Instance.IsShowingOverlayCanvas ||
                InventoryManager.Instance.IsShowingInventory;
        }
    }
}
