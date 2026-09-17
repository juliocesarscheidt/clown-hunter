using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableUIItem : MonoBehaviour {

    public int DefaultOrderItem;
    public enum InteractableType {
        Gun = 0,
        Item = 1,
    }
    public InteractableType interactableType;
    public string UIName;
    public bool ShowOnInventory;

    private void Awake() {
    }

    void Start() {
    }

    void Update() {
    }
}
