using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableUIItem : MonoBehaviour {

    public int DefaultOrderItem;

    public enum InteractibleType {
        Gun = 0,
        Item = 1,
    }
    public InteractibleType interactibleType;

    private void Awake() {
    }

    void Start() {
    }

    void Update() {
    }
}
