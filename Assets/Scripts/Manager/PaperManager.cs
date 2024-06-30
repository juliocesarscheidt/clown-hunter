using System.Collections.Generic;
using UnityEngine;
using static TaskManager;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    public GameObject paperPrefab;

    private PlayerStats playerStats;
    // spawnPoints will be splited by areas
    private Dictionary<int, List<GameObject>> spawnPoints = new();
    public List<GameObject> spawnPointsAreas;

    public int totalPapersToCollect = 5;
    public int monstersToAddOnPaperCollected = 2;

    private int thisTaskIndex = (int) TaskType.FindAndCollectNewspapers;

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

    public void SpawnPapers() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        int spawnAreasQuantity = spawnPointsAreas.Count;
        int diffToSpawn = totalPapersToCollect;

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
                spawnPoint.transform.rotation
            );
            paper.transform.parent = spawnPoint.transform;

            if (paper.TryGetComponent(out Interactable component)) {
                InteractionManager.Instance.AddInteractable(component);
            }
        }
    }

    public void CollectPaper() {
        MonsterManager.Instance.monstersToSpawn += monstersToAddOnPaperCollected;
        MonsterManager.Instance.SpawnEnemies();
        TaskManager.Instance.UpdateTaskProgress(thisTaskIndex, +1);
    }
}
