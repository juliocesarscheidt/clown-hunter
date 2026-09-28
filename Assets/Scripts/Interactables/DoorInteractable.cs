using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Interactable
{
    [SerializeField]
    private bool isLocked = true;
    [SerializeField]
    private bool canBeOpened = false;
    [SerializeField]
    private string keyInventoryItemID;

    private AudioSource doorAudioSource;
    public AudioClip doorLockedSound;

    new void Start() {
        base.Start();
        doorAudioSource = GetComponent<AudioSource>();
    }

    public override void OnInteract() {
        if (!isLocked) {
            // show a message in the UI
            HudManager.Instance.ActivateGeneralInfoText("Door is already unlocked");
            return;
        }
        if (!canBeOpened) {
            // show a message in the UI
            HudManager.Instance.ActivateGeneralInfoText("You don't have the key");
            if (doorAudioSource != null) {
                doorAudioSource.PlayOneShot(doorLockedSound);
            }
            return;
        }
        if (keyInventoryItemID != "") {
            var hasKey = InventoryManager.Instance.HasInventoryItemByKey(keyInventoryItemID);
            if (!hasKey) {
                HudManager.Instance.ActivateGeneralInfoText("You don't have the key");
                if (doorAudioSource != null) {
                    doorAudioSource.PlayOneShot(doorLockedSound);
                }
                return;
            }
            DoorsManager.Instance.UnlockDoor();
        }
    }

    public override void EnableOutline() {
        // no outline
    }

    public override void DisableOutline() {
        // no outline
    }

    public void SetCanBeOpened(bool can) {
        canBeOpened = can;
    }

    public string GetKeyInventoryItemID {
        get { return keyInventoryItemID; }
    }

    public void SetKeyInventoryItemID(string keyID) {
        keyInventoryItemID = keyID;
    }
}
