using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Interactable: MonoBehaviour {
    public abstract void OnInteract();
    public abstract void EnableOutline();
    public abstract void DisableOutline();

    protected PlayerStats playerStats;

    public bool isOutlineEnabled;
    protected Outline outlineScript;
    protected TextMeshPro pressInteractText;

    [SerializeField]
    protected bool isInteractionActive = false;
    public LayerMask obstacleLayer; // the wall/obstacle layer to detect between the interactable and player
    [SerializeField]
    protected bool isBlockedByObstacle = false;
    [SerializeField]
    protected float distanceToPlayer;
    public float distanceToPlayerTrigger = 4f;

    public bool playSoundOnInteract = true;
    public bool destroyOnInteract = true;

    public void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        outlineScript = GetComponentInChildren<Outline>();
        pressInteractText = GetComponentInChildren<TextMeshPro>();

        DisableOutline();
    }

    public void LateUpdate() {
        if (!GlobalGameplayManager.Instance.IsGameplayActiveNotReloading) {
            return;
        }

        Vector3 directionToPlayer = playerStats.transform.position - transform.position;
        distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > distanceToPlayerTrigger) {
            isInteractionActive = false;
            isBlockedByObstacle = false;
        } else {
            Vector3 directionToTarget = playerStats.transform.position - transform.position;

            if (Physics.Raycast(transform.position, directionToTarget.normalized, distanceToPlayer, obstacleLayer)) {
                isInteractionActive = false;
                isBlockedByObstacle = true;
            } else {
                isInteractionActive = true;
                isBlockedByObstacle = false;
            }
        }
    
        if (isInteractionActive) {
            pressInteractText.gameObject.SetActive(true);
            // show object outline
            if (!isOutlineEnabled) {
                EnableOutline();
            }
            if (Input.GetButtonDown("Interact")) {
                OnInteract();
                if (playSoundOnInteract) {
                    InteractionManager.Instance.PlayCollectAudio();
                }
                if (destroyOnInteract) {
                    InteractionManager.Instance.RemoveInteractable(this);
                    Destroy(transform.gameObject);
                }
            }
        } else {
            pressInteractText.gameObject.SetActive(false);
            DisableOutline();
        }
    }

    public void SetInteractText(string text) {
        if (pressInteractText == null) {
            return;
        }
        if (text == null) {
            pressInteractText.text = string.Empty;
        } else {
            pressInteractText.text = text;
        }
    }
}
