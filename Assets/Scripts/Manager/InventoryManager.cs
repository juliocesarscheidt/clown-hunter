using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    private PlayerStats playerStats;

    public Canvas InventoryCanvas;
    public List<GameObject> ItemSlots;
    private Dictionary<int, GameObject> SlotsMap = new();
    public int StartItemSlotIndex = 2;

    public List<InteractableUIItem> InteractibleGuns = new();
    public List<InteractableUIItem> InteractibleItems = new();
    public int ItemsToShow = 5;

    public bool IsShowingInventory = false;
    [SerializeField]
    private bool isScrollingItems = false;
    [SerializeField]
    private float scrollingItemsTimer = 0f;
    private readonly float scrollingItemsInterval = 0.5f;

    public enum MenuType
    {
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
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        for (int i = 0; i < ItemSlots.Count; i++) {
            Debug.Log($"adding item slot {i}");
            SlotsMap.Add(i, ItemSlots[i]);
        }

        UpdateSlots();
    }

    void Update() {
        // not checking for HudManager.Instance.IsPaused
        if (!HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        if (IsShowingInventory) {
            float x = Input.GetAxisRaw("Horizontal");

            if (x != 0 && !isScrollingItems) {
                var increment = (x > 0 ? 1 : -1);
                Debug.Log($"increment {increment}");

                isScrollingItems = true;
                UpdateOrderItems(increment);
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

    public void ChangeMenuType(int newType) {
        menuType = ((MenuType) newType);
        UpdateSlots();
    }

    public void UpdateOrderItems(int increment) {
        Debug.Log($"increment {increment}");

        if (menuType == MenuType.Gun) {
            var gunsSize = InteractibleGuns.Count;
            var shiftGuns = new List<InteractableUIItem>();
            for (int i = 0; i < gunsSize; i++) {
                var next = (i + increment) % gunsSize;
                if (next < 0) {
                    next = gunsSize - 1;
                }
                Debug.Log($"next {next}");
                shiftGuns.Add(InteractibleGuns[next]);
                // shiftGuns[i] = InteractibleGuns[next];
            }
            InteractibleGuns = shiftGuns;

        } else if (menuType == MenuType.Item) {
            var itemsSize = InteractibleItems.Count;
            var shiftItems = new List<InteractableUIItem>();
            for (int i = 0; i < itemsSize; i++)  {
                var next = (i + increment) % itemsSize;
                if (next < 0) {
                    next = itemsSize - 1;
                }
                // Debug.Log($"next {next}");
                shiftItems.Add(InteractibleItems[next]);
                // shiftItems[i] = InteractibleItems[(i + increment) % itemsSize];
            }
            InteractibleItems = shiftItems;
        }

        UpdateSlots();
    }

    private void UpdateSlots() {
        List<InteractableUIItem> items = menuType == MenuType.Gun ? InteractibleGuns : InteractibleItems;
        
        // clear slots
        for (int i = 0; i < ItemsToShow; i++) {
            for (int j = 0; j < ItemSlots[i].transform.childCount; j++) {
                Destroy(ItemSlots[i].transform.GetChild(j).gameObject);
            }
        }

        // foreach (InteractableUIItem item in items) {
        int relativeIndex = 0;
        for (int startingSlotIndex = StartItemSlotIndex; ;) {

            if (items.Count-1 >= relativeIndex) {
                var item = items[relativeIndex];
                Debug.Log($"order {relativeIndex} {item.name}");

                // we cannot show this item
                if (!SlotsMap.ContainsKey(startingSlotIndex)) {
                    Debug.Log($"it doesnt contain order :: {startingSlotIndex}");
                    continue;
                }

                var currentSlot = SlotsMap[startingSlotIndex];
                GameObject newItem = Instantiate(
                    item.gameObject,
                    currentSlot.transform.position,
                    currentSlot.transform.rotation,
                    currentSlot.transform
                );
                newItem.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0f, 15f, 0f));
                newItem.transform.localScale = new Vector3(1, 1, 1);
            }

            startingSlotIndex++;
            if (startingSlotIndex >= ItemSlots.Count-1) {
                startingSlotIndex = 0;
            }

            relativeIndex++;
            if (relativeIndex >= items.Count-1) {
                break;
            }
        }

        //for (int i = 0; i < items.Count; i++) {
        //    var item = items[i];

        //    // int order = item.GetOrderItem;
        //    int order = i;
        //    Debug.Log($"order {order} {item.name}");

        //    // we cannot show this item
        //    if (!SlotsMap.ContainsKey(order)) {
        //        Debug.Log($"it doesnt contain order :: {order}");
        //        continue;
        //    }

        //    var currentSlot = SlotsMap[order];
        //    GameObject newItem = Instantiate(
        //        item.gameObject,
        //        currentSlot.transform.position,
        //        currentSlot.transform.rotation,
        //        currentSlot.transform
        //    );
        //    newItem.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0f, 15f, 0f));
        //    newItem.transform.localScale = new Vector3(1, 1, 1);
        //}
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
        if (item.interactibleType == InteractableUIItem.InteractibleType.Item) {
            InteractibleItems.Add(item);
            InteractibleItems = InteractibleItems.OrderBy(o => o.DefaultOrderItem).ToList();

        } else if (item.interactibleType == InteractableUIItem.InteractibleType.Gun) {
            InteractibleGuns.Add(item);
            InteractibleGuns = InteractibleGuns.OrderBy(o => o.DefaultOrderItem).ToList();
        }

        UpdateSlots();
    }
}
