using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    private PlayerStats playerStats;

    public TextMeshProUGUI paperCounterText;
    public GameObject paperPrefab;
    [SerializeField]
    private List<GameObject> spawnPoints;
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

        foreach (GameObject spawnArea in spawnPointsAreas) {
            for (int i = 0; i < spawnArea.transform.childCount; i++){
                spawnPoints.Add(spawnArea.transform.GetChild(i).gameObject);
            }
        }
        SpawnPapers();
        AdjustPaperCounterText();
    }

    public void SpawnPapers() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        int spawnPointsQuantity = spawnPoints.Count;
        int diffToSpawn = papersToSpawn;

        List<int> randomSpawnPoints = new();
        List<int> spawnedAreas = new();

        int currentRetriesToFindSpawnPoints = 0;

        for (int i = 0; i < diffToSpawn; i++) {
            // get a random spawn point, try to not get a repeated one
            int randomSpawnPointIndex = Random.Range(0, spawnPointsQuantity);

            // split by areas
            GameObject spawnPoint = spawnPoints[randomSpawnPointIndex];
            int area = int.Parse(spawnPoint.transform.parent.name);

            currentRetriesToFindSpawnPoints++;
            if ((
                randomSpawnPoints.Contains(randomSpawnPointIndex) ||
                spawnedAreas.Contains(area)
            ) && currentRetriesToFindSpawnPoints < 20) {
                i--;
                continue;
            }

            randomSpawnPoints.Add(randomSpawnPointIndex);
            spawnedAreas.Add(area);
        }

        for (int i = 0; i < diffToSpawn; i++) {
            GameObject spawnPoint = spawnPoints[randomSpawnPoints[i]];

            // spawn the enemy
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
