using System.Collections.Generic;
using UnityEngine;
using static TaskManager;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    public GameObject paperPrefab;
    [SerializeField]
    private List<GameObject> spawnedPapers = new();
    public int defaultLayerIndex = 0;
    public int paperLayerIndex = 6;

    private PlayerStats playerStats;
    // spawnPoints will be splited by areas
    private Dictionary<int, List<GameObject>> spawnPoints = new();
    public List<GameObject> spawnPointsAreas;

    public int totalPapersToCollect = 4;
    public int monstersToAddOnPaperCollected = 2;

    private readonly int thisTaskIndex = (int) TaskType.CollectTheNewspapers;

    public List<int> sortedNumbers = new();
    // front materials - 01 to 04
    public List<Material> frontMaterials;
    // back materials - 00 to 09
    public List<Material> backMaterials;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        for (int i = 0; i < spawnPointsAreas.Count; i++) {
            GameObject spawnArea = spawnPointsAreas[i];
            List<GameObject> areaSpawnPoints = new();
            for (int j = 0; j < spawnArea.transform.childCount; j++){
                areaSpawnPoints.Add(spawnArea.transform.GetChild(j).gameObject);
            }
            spawnPoints.Add(i, areaSpawnPoints);
        }

        SpawnPapers();

        TaskManager.Instance.UpdateTaskTotalProgress(thisTaskIndex, totalPapersToCollect);
        TaskManager.Instance.UpdateTaskProgress(thisTaskIndex, 0);
    }

    private List<int> shuffleRandomNumbers() {
        List<int> numbers = new();
        for (int i = 0; i < 10; i++) {
            numbers.Add(i);
        }
        for (int i = 0; i < numbers.Count; i++) {
            int randomIndex = Random.Range(i, numbers.Count);
            (numbers[randomIndex], numbers[i]) = (numbers[i], numbers[randomIndex]);
        }
        return numbers;
    }

    public void SpawnPapers() {
        if (InventoryManager.Instance.IsShowingInventory || HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        int spawnAreasQuantity = spawnPointsAreas.Count;
        int diffToSpawn = totalPapersToCollect;

        // shuffle the numbers from 0 to 9 to get random back materials for the papers
        List<int> randomNumbers = shuffleRandomNumbers();
        List<int> randomSpawnAreas = new();
        
        for (int i = 0; i < diffToSpawn; i++) {
            // get a random spawn are, try to not get a repeated one
            int randomSpawnAreaIndex = Random.Range(0, spawnAreasQuantity);
            if (randomSpawnAreas.Contains(randomSpawnAreaIndex)) {
                i--;
                continue;
            }
            randomSpawnAreas.Add(randomSpawnAreaIndex);
        }

        for (int i = 0; i < diffToSpawn; i++) {
            int area = randomSpawnAreas[i];
            // get the count of spawn areas inside the current area
            int spawnPointsCountInsideArea = spawnPoints.GetValueOrDefault(area).Count;
            // get a random spawn point within a given area
            int randomSpawnPointIndex = Random.Range(0, spawnPointsCountInsideArea);
            GameObject spawnPoint = spawnPoints.GetValueOrDefault(area)[randomSpawnPointIndex];
            // spawn the paper
            GameObject paper = Instantiate(
                paperPrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation,
                spawnPoint.transform
            );

            int sortedNumber = randomNumbers[i];
            sortedNumbers.Add(sortedNumber);

            // copying the iterator to a new variable to use inside the lambda function
            int innerIterator = i;

            if (paper.TryGetComponent<Paper>(out var p)) {
                string name = $"Paper #{innerIterator + 1}";
                p.name = name;

                p.assignedSortedNumber = sortedNumber;
                if (frontMaterials.Count > innerIterator) {
                    p.SetFrontMaterial(frontMaterials[innerIterator]);
                }
                if (backMaterials.Count > sortedNumber) {
                    p.SetBackMaterial(backMaterials[sortedNumber]);
                }

                void instantiateAction(InventoryItem item, GameObject prefab) {
                    Renderer rend = prefab.GetComponentInChildren<Renderer>(true);
                    if (rend != null) {
                        Material[] mats = rend.materials;
                        if (mats.Length > 0 && frontMaterials.Count > innerIterator) {
                            mats[0] = frontMaterials[innerIterator];
                        }
                        if (mats.Length > 2 && backMaterials.Count > sortedNumber) {
                            mats[2] = backMaterials[sortedNumber];
                        }
                        rend.materials = mats;
                    }
                }
                // dynamically set inventory item in the paper
                p.SetInventoryItemData(i, name, false, true, null, instantiateAction);
            }

            spawnedPapers.Add(paper);
            if (paper.TryGetComponent(out Interactable component)) {
                InteractionManager.Instance.AddInteractable(component);
            }
        }
    }

    public void CollectPaper(Paper paper) {
        MonsterManager.Instance.monstersToSpawn += monstersToAddOnPaperCollected;
        MonsterManager.Instance.SpawnEnemies();
        // add the UI item to inventory manager
        if (InventoryManager.Instance != null && paper != null) {
            InventoryManager.Instance.AddInteractableItem(paper.inventoryItem, paper.baseInventoryItemPrefab);
        }
        // remove from spawnedPapers
        spawnedPapers.Remove(paper.gameObject);
        // increment task progress
        TaskManager.Instance.UpdateTaskProgress(thisTaskIndex, +1);
    }

    public void ShowAllPapers(bool showAllPapers) {
        // change the layer to Paper
        foreach(var obj in spawnedPapers) {
            if (obj == null || !obj.activeSelf) {
                continue;
            }
            if (obj.transform.childCount == 0) {
                continue;
            }

            if (obj.TryGetComponent<Paper>(out var p)) {
                if (showAllPapers) {
                    p.SetForcedOutlineEnabled();
                    p.SetPaperObjLayer(paperLayerIndex);
                } else {
                    p.SetForcedOutlineDisabled();
                    p.SetPaperObjLayer(defaultLayerIndex);
                }
            }
        }
    }
}
