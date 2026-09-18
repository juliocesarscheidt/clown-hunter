using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager Instance { get; private set; }

    private PlayerStats playerStats;

    public Canvas InventoryCanvas;
    public List<GameObject> ItemSlots;
    private Dictionary<int, GameObject> SlotsMap = new();
    public int CenterSlotIndex = 2;
    private int currentItemSlotIndex;

    public List<InventoryItem> InteractableGuns = new();
    public List<InventoryItem> InteractableItems = new();

    public TextMeshProUGUI uiItemText;

    public bool IsShowingInventory = false;
    [SerializeField]
    private bool isScrollingItems = false;
    [SerializeField]
    private float scrollTimer = 0f;
    [SerializeField]
    private float scrollDuration = 0.2f; // Animation speed in seconds

    private Dictionary<InventoryItem, List<GameObject>> itemPool = new();
    private readonly List<GameObject> activeSpawnedItems = new();

    private Coroutine scrollAnimationCoroutine;

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
        menuType = ((MenuType) newType);
        currentItemSlotIndex = 0;

        ClearPool();
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
            GameObject uiItemObj = GetPooledItem(currentItemData, targetSlot.transform);

            // Apply starting position
            uiItemObj.transform.SetParent(targetSlot.transform, false);
            uiItemObj.transform.SetPositionAndRotation(targetSlot.transform.position, targetSlot.transform.rotation);
            uiItemObj.transform.localPosition = startLocalPos;
            uiItemObj.transform.localScale = Vector3.one;
            uiItemObj.SetActive(true);

            activeSpawnedItems.Add(uiItemObj);
            itemsToAnimate.Add(uiItemObj.transform);
            startPositions.Add(startLocalPos);
            targetPositions.Add(targetLocalPos);
        }

        UpdateItemNameDisplay();

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
            uiItemText.text = activeItem.InventoryDisplayName;
        } else {
            ClearItemNameDisplay();
        }
    }

    private void ClearItemNameDisplay() {
        uiItemText.text = string.Empty; // Clear text if no items exist
    }

    private IEnumerator AnimateSlotSlide(List<Transform> items, List<Vector3> starts, List<Vector3> targets) {
        while (scrollTimer < scrollDuration) {
            scrollTimer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(scrollTimer / scrollDuration);
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
            if (items[i] != null) items[i].localPosition = targets[i];
        }

        scrollTimer = 0f;
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

    private GameObject GetPooledItem(InventoryItem inventoryItem, Transform parent) {
        if (!itemPool.ContainsKey(inventoryItem)) {
            itemPool[inventoryItem] = new List<GameObject>();
        }
        // Search for an inactive object in the pool
        List<GameObject> pool = itemPool[inventoryItem];
        for (int i = 0; i < pool.Count; i++) {
            if (!pool[i].activeInHierarchy) {
                return pool[i];
            }
        }
        // If none are inactive, instantiate a new one and add it to the pool
        // using the base prefab
        GameObject newInstance = Instantiate(inventoryItem.baseInventoryItemPrefab.gameObject, parent);
      
        pool.Add(newInstance);
        return newInstance;
    }

    private List<InventoryItem> currentInteractableItems {
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

    public void AddInteractableItem(InventoryItem item) {
        if (item.displayOnInventory) {
            InventoryItem currentCenterItem = null;
            if (currentInteractableItems != null && currentItemSlotIndex >= 0 && currentItemSlotIndex < currentInteractableItems.Count) {
                currentCenterItem = currentInteractableItems[currentItemSlotIndex];
            }

            if (item.interactableType == InventoryItem.InteractableType.Item) {
                InteractableItems.Add(item);
            } else if (item.interactableType == InventoryItem.InteractableType.Gun) {
                InteractableGuns.Add(item);
            }

            // 2. Add and re-sort the list
            InteractableItems = InteractableItems.OrderBy(o => o.DefaultOrderItem).ToList();
            InteractableGuns = InteractableGuns.OrderBy(o => o.DefaultOrderItem).ToList();

            // 3. Find where the centered item moved to after sorting
            if (currentCenterItem != null) {
                int newIndex = currentInteractableItems.IndexOf(currentCenterItem);
                if (newIndex != -1) {
                    currentItemSlotIndex = newIndex; // Keep the same item centered
                }
            } else {
                // If list was empty before, start at index 0
                currentItemSlotIndex = 0;
            }

            // 4. Clamp safety check
            currentItemSlotIndex = Mathf.Clamp(currentItemSlotIndex, 0, currentInteractableItems.Count - 1);

            // 5. Refresh UI layout with updated indices
            UpdateSlots(0);
        }
    }
}
