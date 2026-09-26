using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager Instance { get; private set; }

    public GameObject InventoryCanvas;

    [Header("Slots")]
    public List<GameObject> ItemSlots;
    private Dictionary<int, GameObject> SlotsMap = new();
    public int CenterSlotIndex = 2;
    public GameObject ItemSlotsPanelWrapper;

    [SerializeField]
    private int currentItemSlotIndex = 0;

    [System.NonSerialized]
    private InventoryItem currentCenterItem;

    [Header("Guns Data List")]
    [System.NonSerialized]
    private readonly List<InventoryItem> InteractableGuns = new();
    [System.NonSerialized]
    private Dictionary<int, GameObject> InventoryGunsOriginalPrefabDict = new();
    [System.NonSerialized]
    private Dictionary<int, GameObject> InventoryGunsLivePrefabDict = new();

    [Header("Items Data List")]
    [System.NonSerialized]
    private readonly List<InventoryItem> InteractableItems = new();
    [System.NonSerialized]
    private Dictionary<int, GameObject> InventoryItemsOriginalPrefabDict = new();
    [System.NonSerialized]
    private Dictionary<int, GameObject> InventoryItemsLivePrefabDict = new();

    public int DefaultPlayerItems = 2; // flashlight and nightvision binocular

    public TextMeshProUGUI uiItemNameText;
    public TextMeshProUGUI uiItemEquipText;
    public TextMeshProUGUI uiItemInvestigateText;

    public bool IsShowingInventory = false;
    [SerializeField]
    private bool isScrollingItems = false;
    [SerializeField]
    private float scrollTimer = 0f;
    [SerializeField]
    private float scrollDuration = 0.2f; // Animation speed in seconds

    [System.NonSerialized]
    private Dictionary<InventoryItem, List<GameObject>> itemPool = new();

    private readonly List<GameObject> activeSpawnedItems = new();
    private Coroutine scrollAnimationCoroutine;

    public ItemInspectController itemInspectorController;
    [SerializeField]
    private bool isInspectingItem;

    public enum MenuType {
        Gun = 0,
        Item = 1,
    }
    public MenuType menuType = MenuType.Gun;
    public MenuNavigableController menuNavigableController;

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

    private void Start() {
        void navigateAction(int index) {
            ChangeMenuType(index);
        }
        menuNavigableController.onSelectButtonAction = navigateAction;

        ClearPool();
        UpdateSlots();
    }

    void OnDisable() {
        ClearPool();
    }

    public void ChangeMenuType(int newIndex) {
        menuType = ((MenuType) newIndex);

        if (isInspectingItem) {
            ExitInspectItem();
        }

        currentItemSlotIndex = 0;

        ClearPool();
        UpdateSlots();
    }

    void Update() {
        if (!GlobalGameplayManager.Instance.IsGameplayActiveForInventory) {
            return;
        }
        
        if (IsShowingInventory) {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if (!isInspectingItem && x != 0 && !isScrollingItems) {
                if (CurrentInteractableItems == null || CurrentInteractableItems.Count <= 0) return;

                int direction = (x > 0 ? 1 : -1);
                int nextIndex = Mathf.Clamp(currentItemSlotIndex + direction, 0, CurrentInteractableItems.Count - 1);

                // Only animate if the index actually changed
                if (nextIndex != currentItemSlotIndex) {
                    currentItemSlotIndex = nextIndex;
                    isScrollingItems = true;
                    UpdateSlots(direction); // Pass increment direction
                }
            }
            if (!isInspectingItem && y != 0) {
                int direction = (y > 0 ? 1 : -1);
                // decrement direction to navigate downwards
                int nextIndex = Mathf.Clamp(MenuTypeInt - direction, 0, 1);
                if (nextIndex != MenuTypeInt) {
                    ChangeMenuType(nextIndex);
                }
            }

            if (currentCenterItem != null) {
                if (!isInspectingItem && Input.GetButtonDown("Interact") && currentCenterItem.canEquip) {
                    currentCenterItem.onEquipAction?.Invoke(currentCenterItem);
                }

                if (Input.GetButtonDown("Jump") && currentCenterItem.canInspect) {
                    if (!isInspectingItem) {
                        EnterInspectItem(currentCenterItem);
                    } else {
                        ExitInspectItem();
                    }
                }
            }
        }
    }

    private void PreSetupSlots() {
        for (int i = 0; i < activeSpawnedItems.Count; i++) {
            activeSpawnedItems[i].SetActive(false);
        }
        activeSpawnedItems.Clear();

        // update current center item
        if (CurrentInteractableItems.Count > currentItemSlotIndex) {
            currentCenterItem = CurrentInteractableItems[currentItemSlotIndex];
        } else {
            currentCenterItem = null;
        }

        UpdateItemNameDisplay();
        uiItemEquipText.gameObject.SetActive(false);
        uiItemInvestigateText.gameObject.SetActive(false);
    }

    private void UpdateSlots(int scrollDirection = 0) {
        PreSetupSlots();

        if (CurrentInteractableItems == null || CurrentInteractableItems.Count <= 0) return;

        List<Transform> itemsToAnimate = new();
        List<Vector3> startPositions = new();
        List<Vector3> targetPositions = new();
        // List<Vector3> targetScales = new();

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
            if (itemIndex < 0 || itemIndex >= CurrentInteractableItems.Count) continue;
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

            InventoryItem currentInventoryItem = CurrentInteractableItems[itemIndex];
            GameObject uiItemObj = GetPooledItem(currentInventoryItem, targetSlot.transform);

            bool isCenterItem = (slotIndex == CenterSlotIndex);
            // Vector3 targetScale = Vector3.one;
            if (isCenterItem) {
                // targetScale = Vector3.one * 1.5f; // Scale up the center item
                uiItemEquipText.gameObject.SetActive(currentInventoryItem.canEquip);
                uiItemInvestigateText.gameObject.SetActive(currentInventoryItem.canInspect);
            }

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
            // targetScales.Add(targetScale);
        }

        UpdateItemNameDisplay();

        if (scrollAnimationCoroutine != null) StopCoroutine(scrollAnimationCoroutine);

        if (scrollDirection != 0) {
            scrollAnimationCoroutine = StartCoroutine(AnimateSlotSlide(itemsToAnimate, startPositions, targetPositions));
        } else {
            // Initial setup (e.g. game start): place items instantly without animating
            isScrollingItems = false;
        }
    }
    
    private IEnumerator AnimateSlotSlide(List<Transform> items, List<Vector3> startPositions, List<Vector3> targetPositions) {
        while (scrollTimer < scrollDuration) {
            scrollTimer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(scrollTimer / scrollDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            for (int i = 0; i < items.Count; i++) {
                if (items[i] != null) {
                    items[i].localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], smoothProgress);
                }
            }
            yield return null;
        }

        // Snap to exact target position at the end
        for (int i = 0; i < items.Count; i++) {
            if (items[i] != null) {
                items[i].localPosition = targetPositions[i];
            }
        }

        scrollTimer = 0f;
        isScrollingItems = false;
    }

    private void UpdateItemNameDisplay() {
        if (uiItemNameText == null) return;
        if (currentCenterItem != null) {
            uiItemNameText.text = currentCenterItem.inventoryDisplayName;
        } else {
            if (CurrentInteractableItems.Count == 0) {
                uiItemNameText.text = "No Items";
            } else {
                // this should not happen
                ClearItemNameDisplay();
            }
        }
    }

    private void ClearItemNameDisplay() {
        uiItemNameText.text = string.Empty; // Clear text if no items exist
    }

    public void AddInteractableItem(InventoryItem item, GameObject baseInventoryItemPrefab) {
        if (item != null && item.displayOnInventory) {
            if (item.interactableType == InventoryItem.InteractableType.Item) {
                InteractableItems.Add(item);
                // not sorting items by design, to make them unordered
                // InteractableItems.Sort((a, b) => a.defaultItemIndex.CompareTo(b.defaultItemIndex));

                InventoryItemsOriginalPrefabDict.Add(item.defaultItemIndex, baseInventoryItemPrefab);
                if (menuType == MenuType.Item) {
                    if (currentItemSlotIndex >= 0 && InteractableItems.Count > currentItemSlotIndex) {
                        currentCenterItem = InteractableItems[currentItemSlotIndex];
                    }
                }

            } else if (item.interactableType == InventoryItem.InteractableType.Gun) {
                InteractableGuns.Add(item);
                InteractableGuns.Sort((a, b) => a.defaultItemIndex.CompareTo(b.defaultItemIndex));

                InventoryGunsOriginalPrefabDict.Add(item.defaultItemIndex, baseInventoryItemPrefab);
                if (menuType == MenuType.Gun) {
                    if (currentItemSlotIndex >= 0 && InteractableGuns.Count > currentItemSlotIndex) {
                        currentCenterItem = InteractableGuns[currentItemSlotIndex];
                    }
                }
            }

            // Find where the centered item moved to after sorting
            if (currentCenterItem != null) {
                int newIndex = CurrentInteractableItems.IndexOf(currentCenterItem);
                if (newIndex != -1) {
                    currentItemSlotIndex = newIndex; // Keep the same item centered
                }
            } else {
                // If list was empty before, start at index 0
                currentItemSlotIndex = 0;
            }

            // Clamp safety check
            currentItemSlotIndex = Mathf.Clamp(currentItemSlotIndex, 0, CurrentInteractableItems.Count - 1);

            // Refresh UI layout with updated indices
            UpdateSlots(0);
        }
    }

    private List<InventoryItem> CurrentInteractableItems {
        get {
            return menuType == MenuType.Item ? InteractableItems : InteractableGuns;
        }
    }
    
    public void EnterInspectItem(InventoryItem inventoryItem) {
        isInspectingItem = true;
        ItemSlotsPanelWrapper.SetActive(false);

        GameObject prefab;
        if (inventoryItem.interactableType == InventoryItem.InteractableType.Item) {
            if (InventoryItemsLivePrefabDict.ContainsKey(inventoryItem.defaultItemIndex)) {
                prefab = InventoryItemsLivePrefabDict[inventoryItem.defaultItemIndex];
            } else {
                prefab = InventoryItemsOriginalPrefabDict[inventoryItem.defaultItemIndex];
            }
        } else {
            if (InventoryGunsLivePrefabDict.ContainsKey(inventoryItem.defaultItemIndex)) {
                prefab = InventoryGunsLivePrefabDict[inventoryItem.defaultItemIndex];
            } else {
                prefab = InventoryGunsOriginalPrefabDict[inventoryItem.defaultItemIndex];
            }
        }
        if (prefab != null) {
            // item inspector controller
            itemInspectorController.OpenInspectView(prefab);
            // unlocks cursor
            HudManager.Instance.InventoryEnterInspectMode();
            // disable menu buttons
            menuNavigableController.ToggleMenuButtons(false);
        }
    }
 
    public void ExitInspectItem() {
        isInspectingItem = false;
        ItemSlotsPanelWrapper.SetActive(true);
        // item inspector controller
        itemInspectorController.CloseInspectView();
        // locks cursor
        HudManager.Instance.InventoryExitInspectMode();
        // enable menu buttons
        menuNavigableController.ToggleMenuButtons(true);
        // select the current menu button
        menuNavigableController.SelectCurrentMenuButton();
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
        // getting the base prefab
        GameObject prefab;
        if (inventoryItem.interactableType == InventoryItem.InteractableType.Item) {
            prefab = InventoryItemsOriginalPrefabDict[inventoryItem.defaultItemIndex];
        } else {
            prefab = InventoryGunsOriginalPrefabDict[inventoryItem.defaultItemIndex];
        }

        // If none are inactive, instantiate a new one and add it to the pool
        GameObject newInstance = Instantiate(prefab, parent);
        // call the on instantiate action
        inventoryItem.onInstantiateAction?.Invoke(inventoryItem, newInstance);

        // add the prefab in the live dictionary with the new instance with changed materials - ONLY for items
        if (inventoryItem.interactableType == InventoryItem.InteractableType.Item) {
            if (InventoryItemsLivePrefabDict.ContainsKey(inventoryItem.defaultItemIndex)) {
                InventoryItemsLivePrefabDict[inventoryItem.defaultItemIndex] = newInstance;
            } else {
                InventoryItemsLivePrefabDict.Add(inventoryItem.defaultItemIndex, newInstance);
            }
        } else {
            if (InventoryGunsLivePrefabDict.ContainsKey(inventoryItem.defaultItemIndex)) {
                InventoryGunsLivePrefabDict[inventoryItem.defaultItemIndex] = newInstance;
            } else {
                InventoryGunsLivePrefabDict.Add(inventoryItem.defaultItemIndex, newInstance);
            }
        }

        pool.Add(newInstance);
        return newInstance;
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

    private int MenuTypeInt {
        get { return (int) menuType; }
    }

    public void ShowInventoryPanel() {
        IsShowingInventory = true;
        InventoryCanvas.SetActive(true);
    }

    public void HideInventoryPanel() {
        if (isInspectingItem) {
            ExitInspectItem();
        }
        IsShowingInventory = false;
        InventoryCanvas.SetActive(false);
    }
}
