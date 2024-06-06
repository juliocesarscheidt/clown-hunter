using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy").Length;

        int enemiesPrefabsQuantity = enemiesPrefabs.Count;
        int spawnPointsQuantity = spawnPoints.Count;
        int diffEnemiesToSpawn = Mathf.Max(enemiesToSpawn - enemiesAlive, 0);

        // Debug.Log("enemiesAlive " + enemiesAlive);
        // Debug.Log("spawnPointsQuantity " + spawnPointsQuantity);
        // Debug.Log("diffEnemiesToSpawn " + diffEnemiesToSpawn);

        List<int> spawnPointsCounter = new();

        int currentRetriesToFindSpawnPoints = 0;

        for (int i = 0; i < diffEnemiesToSpawn; i++) {
            // get a random spawn point, try to not get a repeated one
            int randomSpawnPointIndex = Random.Range(0, spawnPointsQuantity);

            currentRetriesToFindSpawnPoints++;

            float distanceToPlayer = Vector3.Distance(
                spawnPoints[randomSpawnPointIndex].transform.position,
                playerStats.transform.position);

            if (distanceToPlayer <= 20f) {
                i--;
                continue;
            } else if (spawnPointsCounter.Contains(randomSpawnPointIndex)
                && currentRetriesToFindSpawnPoints < 10) {
                i--;
                continue;
            }

            spawnPointsCounter.Add(randomSpawnPointIndex);
        }

        // round robin index
        int currentEnemySpawnedIndex = lastEnemySpawnedIndex;
        // Debug.Log("currentEnemySpawnedIndex " + currentEnemySpawnedIndex);

        for (int i = 0; i < diffEnemiesToSpawn; i++) {
            GameObject spawnPoint = spawnPoints[spawnPointsCounter[i]];

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
