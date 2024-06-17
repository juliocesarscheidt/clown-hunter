using UnityEngine;
public class Paper : Interactable
{
    private Outline outlineScript;

    private void Start() {
        outlineScript = GetComponentInChildren<Outline>();
        DisableOutline();
    }

    public override void Collect() {
        HudManager.Instance.HidePressEObject();
        PaperManager.Instance.CollectPaper();
    }

    public override void EnableOutline() {
        outlineScript.enabled = true;
    }

    public override void DisableOutline() {
        outlineScript.enabled = false;
    }
}
