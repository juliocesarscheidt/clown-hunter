using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    private PlayerStats playerStats;

    public TextMeshProUGUI paperCounterText;
    public GameObject paperPrefab;

    // spawnPoints will be splited by areas
    public Dictionary<int, List<GameObject>> spawnPoints = new();
    public List<GameObject> spawnPointsAreas;

    public int papersToSpawn = 5;
    public int papersTotalToCollect = 5;
    private int paperCollected = 0;

    public int enemiesToIncreaseOnPaperCollected = 2;

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
        AdjustPaperCounterText();
    }

    public void SpawnPapers() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        int spawnAreasQuantity = spawnPointsAreas.Count;
        int diffToSpawn = papersToSpawn;

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
        }
    }

    public void CollectPaper() {
        paperCollected++;
        AdjustPaperCounterText();

        MonsterManager.Instance.enemiesToSpawn += enemiesToIncreaseOnPaperCollected;
        MonsterManager.Instance.SpawnEnemiesDelayed();

        if (paperCollected >= papersTotalToCollect) {
            HudManager.Instance.ShowEndGameImage();
        }
    }

    public void AdjustPaperCounterText() {
        paperCounterText.text = paperCollected + "/" + papersTotalToCollect;
    }
}
