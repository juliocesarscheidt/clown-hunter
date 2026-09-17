using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager Instance { get; private set; }

    private PlayerStats playerStats;

    public Canvas InventoryCanvas;
    public List<GameObject> ItemSlots;
    private Dictionary<int, GameObject> SlotsMap = new();
    public int CenterSlotIndex = 2;
    private int currentItemSlotIndex;

    public List<InteractableUIItem> InteractableGuns = new();
    public List<InteractableUIItem> InteractableItems = new();

    public TextMeshProUGUI uiItemText;

    public bool IsShowingInventory = false;
    [SerializeField]
    private bool isScrollingItems = false;
    [SerializeField]
    private float scrollingItemsTimer = 0f;
    private readonly float scrollingItemsInterval = 0.2f;

    private Dictionary<InteractableUIItem, List<GameObject>> itemPool = new();
    private List<GameObject> activeSpawnedItems = new();

    private Coroutine scrollAnimationCoroutine;
    [SerializeField]
    private float scrollDuration = 0.2f; // Animation speed in seconds

    public enum MenuType {
        Gun = 0,
        Item = 1,
    }
    public MenuType menuType;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        for (int i = 0; i < ItemSlots.Count; i++) {
            SlotsMap.Add(i, ItemSlots[i]);
        }
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        ChangeMenuType(((int)MenuType.Gun));
    }

    public void ChangeMenuType(int newType) {
        Debug.Log($"newType {newType}");

        menuType = ((MenuType) newType);
        currentItemSlotIndex = 0;
        UpdateSlots();
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }
        
        if (IsShowingInventory) {
            float x = Input.GetAxisRaw("Horizontal");

            if (x != 0 && !isScrollingItems) {
                if (currentInteractableItems == null || currentInteractableItems.Count <= 0) return;

                int increment = (x > 0 ? 1 : -1);
                int nextIndex = Mathf.Clamp(currentItemSlotIndex + increment, 0, currentInteractableItems.Count - 1);

                // Only animate if the index actually changed
                if (nextIndex != currentItemSlotIndex) {
                    currentItemSlotIndex = nextIndex;
                    isScrollingItems = true;
                    UpdateSlots(increment); // Pass increment direction
                }
            }

            if (isScrollingItems) {
                scrollingItemsTimer += Time.unscaledDeltaTime;
                if (scrollingItemsTimer > scrollingItemsInterval) {
                    isScrollingItems = false;
                    scrollingItemsTimer = 0f;
                }
            }
        }
    }

    void OnDisable() {
        ClearPool();
    }

    private void UpdateSlots(int scrollDirection = 0) {
        // 1. Deactivate all currently active items instead of Destroying them
        for (int i = 0; i < activeSpawnedItems.Count; i++) {
            activeSpawnedItems[i].SetActive(false);
        }
        activeSpawnedItems.Clear();
        ClearItemNameDisplay();

        if (currentInteractableItems == null || currentInteractableItems.Count <= 0) return;

        List<Transform> itemsToAnimate = new();
        List<Vector3> startPositions = new();
        List<Vector3> targetPositions = new();

        for (int slotIndex = 0; slotIndex < ItemSlots.Count; slotIndex++) {
            // --- VISIBILITY HARD BOUNDARY ---
            // Slots 0, 1, 2, 3, 4 are visible (Center is Slot 2).
            // Slots 5 and up are strictly disabled / hidden.
            if (slotIndex < 0 || slotIndex > 4) {
                continue;
            }

            // Calculate item index based on displacement from StartItemSlotIndex
            int offsetFromCenter = slotIndex - CenterSlotIndex;
            int itemIndex = currentItemSlotIndex + offsetFromCenter;

            // Skip slots that fall outside the bounds of available items
            if (itemIndex < 0 || itemIndex >= currentInteractableItems.Count) continue;
            if (!SlotsMap.TryGetValue(slotIndex, out var targetSlot)) continue;

            // Target position is always the local center of its target slot (Vector3.zero)
            Vector3 targetLocalPos = Vector3.zero;
            Vector3 startLocalPos = Vector3.zero;

            // If scrolling, calculate start position based on where this item came from (previous slot)
            if (scrollDirection != 0) {
                int previousSlotIndex = slotIndex + scrollDirection;

                if (SlotsMap.TryGetValue(previousSlotIndex, out var previousSlot)) {
                    // Convert previous slot position into target slot's local space
                    startLocalPos = targetSlot.transform.InverseTransformPoint(previousSlot.transform.position);
                } else {
                    // If sliding in from off-screen (beyond slot range), extrapolate relative slot distance
                    int outerSlotIndex = slotIndex + (scrollDirection > 0 ? 1 : -1);
                    if (SlotsMap.TryGetValue(outerSlotIndex, out var neighborSlot)) {
                        Vector3 offset = targetSlot.transform.position - neighborSlot.transform.position;
                        startLocalPos = targetSlot.transform.InverseTransformPoint(targetSlot.transform.position + offset);
                    }
                }
            }

            var currentItemData = currentInteractableItems[itemIndex];
            GameObject uiItem = GetPooledItem(currentItemData, targetSlot.transform);

            // update the text with item's name
            UpdateItemNameDisplay();

            // Apply starting position
            uiItem.transform.SetParent(targetSlot.transform, false);
            uiItem.transform.SetPositionAndRotation(targetSlot.transform.position, targetSlot.transform.rotation);
            uiItem.transform.localPosition = startLocalPos;
            uiItem.transform.localScale = Vector3.one;
            uiItem.SetActive(true);

            activeSpawnedItems.Add(uiItem);
            itemsToAnimate.Add(uiItem.transform);
            startPositions.Add(startLocalPos);
            targetPositions.Add(targetLocalPos);
        }

        // 3. Start smooth sliding animation
        if (scrollAnimationCoroutine != null) StopCoroutine(scrollAnimationCoroutine);

        if (scrollDirection != 0) {
            scrollAnimationCoroutine = StartCoroutine(AnimateSlotSlide(itemsToAnimate, startPositions, targetPositions));
        } else {
            // Initial setup (e.g. game start): place items instantly without animating
            isScrollingItems = false;
        }
    }

    private void UpdateItemNameDisplay() {
        if (uiItemText == null) return;
        if (currentInteractableItems != null && currentItemSlotIndex >= 0 && currentItemSlotIndex < currentInteractableItems.Count) {
            var activeItem = currentInteractableItems[currentItemSlotIndex];
            uiItemText.text = activeItem.UIName;
        } else {
            ClearItemNameDisplay();
        }
    }

    private void ClearItemNameDisplay() {
        uiItemText.text = string.Empty; // Clear text if no items exist
    }

    private IEnumerator AnimateSlotSlide(List<Transform> items, List<Vector3> starts, List<Vector3> targets) {
        float elapsedTime = 0f;
        while (elapsedTime < scrollDuration) {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / scrollDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < items.Count; i++) {
                if (items[i] != null) {
                    items[i].localPosition = Vector3.Lerp(starts[i], targets[i], smoothProgress);
                }
            }
            yield return null;
        }

        // Snap to exact target position at the end
        for (int i = 0; i < items.Count; i++) {
            if (items[i] != null) {
                items[i].localPosition = targets[i];
            }
        }
        isScrollingItems = false;
    }

    public void ClearPool() {
        // Destroy all pooled objects
        foreach (var poolList in itemPool.Values) {
            for (int i = 0; i < poolList.Count; i++) {
                if (poolList[i] != null) {
                    Destroy(poolList[i]);
                }
            }
        }
        itemPool.Clear();
        activeSpawnedItems.Clear();
    }

    private GameObject GetPooledItem(InteractableUIItem itemPrefab, Transform parent) {
        if (!itemPool.ContainsKey(itemPrefab)) {
            itemPool[itemPrefab] = new List<GameObject>();
        }
        // Search for an inactive object in the pool
        List<GameObject> pool = itemPool[itemPrefab];
        for (int i = 0; i < pool.Count; i++) {
            if (!pool[i].activeInHierarchy) {
                return pool[i];
            }
        }
        // If none are inactive, instantiate a new one and add it to the pool
        GameObject newInstance = Instantiate(itemPrefab.gameObject, parent);
        pool.Add(newInstance);
        return newInstance;
    }

    private List<InteractableUIItem> currentInteractableItems {
        get { return menuType == MenuType.Gun ? InteractableGuns : InteractableItems; }
    }

    public void ShowInventoryPanel() {
        IsShowingInventory = true;
        InventoryCanvas.gameObject.SetActive(IsShowingInventory);
    }

    public void HideInventoryPanel() {
        IsShowingInventory = false;
        InventoryCanvas.gameObject.SetActive(IsShowingInventory);
    }

    public void AddInteractableItem(InteractableUIItem item) {
        if (item.ShowOnInventory) {
            if (item.interactableType == InteractableUIItem.InteractableType.Item) {
                InteractableItems.Add(item);
            } else if (item.interactableType == InteractableUIItem.InteractableType.Gun) {
                InteractableGuns.Add(item);
            }
        }

        InteractableItems = InteractableItems.OrderBy(o => o.DefaultOrderItem).ToList();
        InteractableGuns = InteractableGuns.OrderBy(o => o.DefaultOrderItem).ToList();

        UpdateSlots();
    }
}
