using UnityEngine;

public class Paper : Interactable {
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;

    private new void Start() {
        base.Start();
        paperObj = transform.GetChild(0).gameObject;
    }

    public override void Collect() {
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
