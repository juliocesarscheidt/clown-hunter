using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TaskManager;

public class DoorsManager : MonoBehaviour {
    public static DoorsManager Instance { get; private set; }

    public List<GameObject> Doors = new();
    [SerializeField]
    private int sortedDoorIndex;

    private readonly int thisTaskIndex = (int) TaskType.EscapeFromThePlace;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        // sort a random door each time
        sortedDoorIndex = Random.Range(0, Doors.Count);
    }

    public void SetupDoors() {
        if (Doors.Count > sortedDoorIndex) {
            if (Doors[sortedDoorIndex].TryGetComponent<DoorInteractable>(out var door)) {
                door.SetCanBeOpened(true);
                if (TreasureChestManager.Instance != null) {
                    door.SetKeyInventoryItemID(TreasureChestManager.Instance.KeyInventoryItemID);
                }
            }
        }
    }

    public void UnlockDoor() {
        TaskManager.Instance.UpdateTaskProgress(thisTaskIndex, +1);
    }
}
