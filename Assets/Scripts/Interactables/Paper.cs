using UnityEngine;

public class Paper : Interactable
{
    private Outline outlineScript;

    private void Start() {
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();
    }

    public override void Collect() {
        HudManager.Instance.HidePressInteractObject();
        PaperManager.Instance.CollectPaper();
    }

    public override void EnableOutline() {
        isOutlineEnabled = true;
        outlineScript.enabled = isOutlineEnabled;
    }

    public override void DisableOutline() {
        isOutlineEnabled = false;
        outlineScript.enabled = isOutlineEnabled;
    }
}
