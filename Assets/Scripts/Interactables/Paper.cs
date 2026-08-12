using UnityEngine;

public class Paper : Interactable
{
    private Outline outlineScript;
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;

    private void Start() {
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();

        paperObj = transform.GetChild(0).gameObject;
    }

    public override void Collect() {
        HudManager.Instance.HidePressInteractObject();
        PaperManager.Instance.CollectPaper();
    }

    public void SetPaperObjLayer(int layer) {
        if (paperObj != null) {
            paperObj.layer = layer;
        }
    }

    public void SetForcedOutlineEnabled() {
        forcedOutlineEnabled = true;
        EnableOutline();
    }

    public void SetForcedOutlineDisabled() {
        forcedOutlineEnabled = false;
        DisableOutline();
    }

    public override void EnableOutline() {
        isOutlineEnabled = true;
        outlineScript.enabled = isOutlineEnabled;
    }

    public override void DisableOutline() {
        if (!forcedOutlineEnabled) {
            isOutlineEnabled = false;
            outlineScript.enabled = isOutlineEnabled;
        }
    }
}
