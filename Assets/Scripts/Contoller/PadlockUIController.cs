using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadlockUIController : MonoBehaviour {
    private PlayerStats playerStats;

    [Header("Roulettes")]
    public List<GameObject> combinationRoulettes = new();
    private List<Outline> combinationRouletteOutlines = new();
    [SerializeField]
    private int currentItemSlotIndex;
    public float defaultRouletteRotation = 36f;
    // 3 8 3 8 - initial combination
    [SerializeField]
    private List<int> combinationNumbers = new() { 3, 8, 3, 8 };

    [Header("Navigation")]
    [SerializeField]
    private bool isNavigatingRoulettes;
    public float navigationDuration = 0.2f;
    [SerializeField]
    private float navigationTimer = 0f;

    [Header("Scrolling")]
    [SerializeField]
    private bool isScrollingRoulettes;
    public float scrollingDuration = 0.2f;
    [SerializeField]
    private float scrollingTimer = 0f;

    private void Awake() {
        playerStats = FindObjectOfType<PlayerStats>();

        for (int i = 0; i < combinationRoulettes.Count; i++) {
            if (combinationRoulettes[i].TryGetComponent<Outline>(out var outlineScript)) {
                combinationRouletteOutlines.Add(outlineScript);
            }
        }
    }

    void Start() {
        if (combinationRoulettes.Count > 0) {
            // disable all outlines and enable outline for the first roulette
            currentItemSlotIndex = 0;
            EnableOutlineAtIndex(currentItemSlotIndex);
        }
    }

    void Update() {
        if (!GlobalGameplayManager.Instance.IsGameplayActiveForOverlayCanvas) {
            return;
        }

        if (HudManager.Instance.IsShowingOverlayCanvas && TreasureChestManager.Instance.IsLocked) {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            
            if (x != 0 && !isNavigatingRoulettes && !isScrollingRoulettes) {
                int direction = (x > 0 ? 1 : -1);
                var currentRoulette = combinationRoulettes[currentItemSlotIndex];
                currentRoulette.transform.Rotate(Vector3.up, direction * defaultRouletteRotation);
                isScrollingRoulettes = true;
                ChangeCombinationNumbers(currentItemSlotIndex, direction);
            }

            if (y != 0 && !isNavigatingRoulettes && !isScrollingRoulettes) {
                int direction = (y > 0 ? 1 : -1);
                // decrement direction to navigate downwards
                int nextIndex = Mathf.Clamp(currentItemSlotIndex - direction, 0, combinationRoulettes.Count - 1);
                if (nextIndex != currentItemSlotIndex) {
                    currentItemSlotIndex = nextIndex;
                    isNavigatingRoulettes = true;
                    EnableOutlineAtIndex(currentItemSlotIndex); // Pass increment direction
                }
            }

            if (isNavigatingRoulettes) {
                navigationTimer += Time.unscaledDeltaTime;
                if (navigationTimer >= navigationDuration) {
                    isNavigatingRoulettes = false;
                    navigationTimer = 0f;
                }
            }

            if (isScrollingRoulettes) {
                scrollingTimer += Time.unscaledDeltaTime;
                if (scrollingTimer >= scrollingDuration) {
                    isScrollingRoulettes = false;
                    scrollingTimer = 0f;
                }
            }
        }
    }

    private void ChangeCombinationNumbers(int index, int direction) {
        if (index < 0 || index >= combinationNumbers.Count) {
            return;
        }
        // 10 numbers on the roulette, so we clamp between 0 and 9
        int nextNumber = combinationNumbers[currentItemSlotIndex] - direction;
        if (nextNumber > 9) {
            nextNumber = 0;
        }
        if (nextNumber < 0) {
            nextNumber = 9;
        }
        combinationNumbers[index] = nextNumber;

        TreasureChestManager.Instance.CompareCombinationNumbers(combinationNumbers);
    }

    private void EnableOutlineAtIndex(int index) {
        DisableAllOutlines();
        if (combinationRoulettes.Count > index) {
            combinationRouletteOutlines[index].enabled = true;
        }
    }

    private void DisableAllOutlines() {
        for (int i = 0; i < combinationRouletteOutlines.Count; i++) {
            combinationRouletteOutlines[i].enabled = false;
        }
    }
}
