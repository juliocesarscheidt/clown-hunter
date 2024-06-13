using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    private PlayerStats playerStats;

    public List<GameObject> spawnPoints;
    public List<GameObject> enemiesPrefabs;

    public int enemiesToSpawn = 3;
    private int enemiesAlive = 0;
    private int lastEnemySpawnedIndex = 0;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        SpawnEnemies();
    }

    public IEnumerator SpawnEnemiesAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        SpawnEnemies();
    }

    public void SpawnEnemiesDelayed() {
        StartCoroutine(SpawnEnemiesAfterSeconds(2f));
    }

    public void SpawnEnemies() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy").Length;

        int enemiesPrefabsQuantity = enemiesPrefabs.Count;
        int spawnPointsQuantity = spawnPoints.Count;
        int diffToSpawn = Mathf.Max(enemiesToSpawn - enemiesAlive, 0);

        List<int> randomSpawnPoints = new();

        int currentRetriesToFindSpawnPoints = 0;

        for (int i = 0; i < diffToSpawn; i++) {
            // get a random spawn point, try to not get a repeated one
            int randomSpawnPointIndex = Random.Range(0, spawnPointsQuantity);

            currentRetriesToFindSpawnPoints++;

            float distanceToPlayer = Vector3.Distance(
                spawnPoints[randomSpawnPointIndex].transform.position,
                playerStats.transform.position);

            if (distanceToPlayer <= 25f) {
                i--;
                continue;
            } else if (randomSpawnPoints.Contains(randomSpawnPointIndex)
                && currentRetriesToFindSpawnPoints < 20) {
                i--;
                continue;
            }

            randomSpawnPoints.Add(randomSpawnPointIndex);
        }

        // round robin index
        int currentEnemySpawnedIndex = lastEnemySpawnedIndex;
        // Debug.Log("currentEnemySpawnedIndex " + currentEnemySpawnedIndex);

        for (int i = 0; i < diffToSpawn; i++) {
            GameObject spawnPoint = spawnPoints[randomSpawnPoints[i]];

            // spawn the enemy
            Instantiate(
                enemiesPrefabs[currentEnemySpawnedIndex],
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );

            currentEnemySpawnedIndex++;
            if (currentEnemySpawnedIndex >= enemiesPrefabsQuantity) {
                currentEnemySpawnedIndex = 0;
            }

            lastEnemySpawnedIndex = currentEnemySpawnedIndex;

            enemiesAlive++;
        }
    }
}
