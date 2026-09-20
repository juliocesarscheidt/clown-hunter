using Unity.Burst.CompilerServices;
using UnityEngine;

public class Paper : InteractableInventory {
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;
    
    private void Awake() {
        paperObj = transform.GetChild(0).gameObject;
    }

    public override void Collect() {
        PaperManager.Instance.CollectPaper(this);
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
