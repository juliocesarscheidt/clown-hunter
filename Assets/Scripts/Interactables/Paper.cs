using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Paper : InteractableInventory {
    private bool forcedOutlineEnabled = false;
    private GameObject paperObj;
    private Renderer rend;
    private Material[] materials;
    public int assignedSortedNumber;

    private void Awake() {
        paperObj = transform.GetChild(0).gameObject;
        rend = paperObj.GetComponent<Renderer>();
        materials = rend.materials;
    }

    public override void OnInteract() {
        PaperManager.Instance.CollectPaper(this);
    }

    public void SetFrontMaterial(Material mat) {
        if (materials.Count() > 0) {
            materials[0] = mat;
        }
        rend.materials = materials;
    }

    public void SetBackMaterial(Material mat) {
        if (materials.Count() > 2) {
            materials[2] = mat;
        }
        rend.materials = materials;
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
