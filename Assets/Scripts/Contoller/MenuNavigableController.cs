using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigableController : MonoBehaviour {

    public List<Button> buttons;
    public bool canNavigate = true;
    public bool selectFirstButtonOnEnable = true;
    [SerializeField]
    private int selectedButtonIndex;

    private Coroutine selectMenuButtonCoroutine;

    [System.NonSerialized]
    public Action<int> onSelectButtonAction;

    void Update() {
        float y = Input.GetAxisRaw("Vertical");

        if (canNavigate && y != 0) {
            int direction = (y > 0 ? 1 : -1);
            // decrement direction to navigate downwards
            int nextIndex = Mathf.Clamp(selectedButtonIndex - direction, 0, buttons.Count-1);

            if (nextIndex != selectedButtonIndex && buttons.Count > nextIndex && buttons[nextIndex] != null) {
                if (selectMenuButtonCoroutine != null) {
                    StopCoroutine(selectMenuButtonCoroutine);
                }
                selectMenuButtonCoroutine = StartCoroutine(SelectMenuButtonAtIndexDelayed(nextIndex));
            }
        }
    }

    public void SelectCurrentMenuButton() {
        SelectMenuButtonAtIndex(selectedButtonIndex);
    }

    public void OnEnable() {
        if (selectFirstButtonOnEnable) {
            if (selectMenuButtonCoroutine != null) {
                StopCoroutine(selectMenuButtonCoroutine);
            }
            selectMenuButtonCoroutine = StartCoroutine(SelectMenuButtonAtIndexDelayed(selectedButtonIndex));
        }
    }

    public void SelectMenuButtonAtIndex(int index) {
        if (buttons.Count > index && buttons[index] != null) {
            selectedButtonIndex = index;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }
        if (index != selectedButtonIndex) {
            onSelectButtonAction?.Invoke(index);
        }
    }

    public IEnumerator SelectMenuButtonAtIndexDelayed(int index) {
        if (buttons.Count > index && buttons[index] != null) {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null; // Wait one frame
            selectedButtonIndex = index;
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }
        if (index != selectedButtonIndex) {
            onSelectButtonAction?.Invoke(index);
        }
    }

    public void ToggleMenuButtons(bool enabled) {
        canNavigate = enabled;

        if (enabled) {
            for (int i = 0; i < buttons.Count; i++) {
                buttons[i].enabled = true;
                buttons[i].interactable = true;
            }
        } else {
            buttons[selectedButtonIndex].interactable = false;
            for (int i = 0; i < buttons.Count; i++) {
                if (i != selectedButtonIndex) {
                    buttons[i].enabled = false;
                }
            }
        }
    }
}
