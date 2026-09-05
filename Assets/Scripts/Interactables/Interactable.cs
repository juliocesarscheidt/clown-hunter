using TMPro;
using UnityEngine;

public abstract class Interactable: MonoBehaviour {
    public abstract void Collect();
    public abstract void EnableOutline();
    public abstract void DisableOutline();

    protected PlayerStats playerStats;

    public bool isOutlineEnabled;
    protected Outline outlineScript;
    [SerializeField]
    protected TextMeshPro pressInteractText;

    public void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        outlineScript = GetComponentInChildren<Outline>();
        pressInteractText = GetComponentInChildren<TextMeshPro>();
        DisableOutline();
    }

    [SerializeField]
    protected float distanceToPlayer;
    public float distanceToPlayerTrigger = 5f;

    public void LateUpdate() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead || playerStats.isReloading) {
            return;
        }

        distanceToPlayer = Vector3.Distance(transform.position, playerStats.transform.position);
        if (distanceToPlayer > distanceToPlayerTrigger) {
            pressInteractText.gameObject.SetActive(false);
            DisableOutline();

        } else {
            pressInteractText.gameObject.SetActive(true);

            if (transform.gameObject.TryGetComponent(out Interactable obj)) {
                // show object outline
                if (!obj.isOutlineEnabled) {
                    obj.EnableOutline();
                }

                if (Input.GetButtonDown("Interact")) {
                    obj.Collect();
                    InteractionManager.Instance.PlayCollectAudio();
                    InteractionManager.Instance.RemoveInteractable(obj);

                    Destroy(transform.gameObject);
                }
            }
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
