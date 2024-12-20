using UnityEngine;

public abstract class Interactable: MonoBehaviour {

    public bool isOutlineEnabled;

    public abstract void Collect();
    public abstract void EnableOutline();
    public abstract void DisableOutline();
}
