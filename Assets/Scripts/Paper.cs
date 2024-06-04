using UnityEngine;
public class Paper : MonoBehaviour, Interactable
{
    public void Collect() {
        HudManager.Instance.HidePressEObject();
        PaperManager.Instance.CollectPaper();
    }
}
