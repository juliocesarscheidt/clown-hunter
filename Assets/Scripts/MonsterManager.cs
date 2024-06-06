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

    public void SpawnEnemies() {
        enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        // Debug.Log("enemiesAlive " + enemiesAlive);

        int enemiesQuantity = enemiesPrefabs.Count;

        int enemiesToSpawnRatio = Mathf.CeilToInt(
           float.Parse(enemiesToSpawn.ToString()) / float.Parse(enemiesQuantity.ToString()));
        // Debug.Log("enemiesToSpawnRatio " + enemiesToSpawnRatio);
        int[] enemiesSpawnedCounter = new int[enemiesQuantity];

        int spawnPointsQuantity = spawnPoints.Count;
        // Debug.Log("spawnPointsQuantity " + spawnPointsQuantity);

        int diffEnemiesToSpawn = Mathf.Max(enemiesToSpawn - enemiesAlive, 0);
        // Debug.Log("diffEnemiesToSpawn " + diffEnemiesToSpawn);

        List<int> spawnPointsCounter = new();

        int currentRetriesToFindSpawnPoints = 0;
        for (int i = 0; i < diffEnemiesToSpawn; i++) {
            // get a random spawn point, try to not get a repeated one
            int randomSpawnPointIndex = Random.Range(0, spawnPointsQuantity);

            currentRetriesToFindSpawnPoints++;
            //Debug.Log("currentRetriesToFindSpawnPoints " + currentRetriesToFindSpawnPoints);

            float distanceToPlayer = Vector3.Distance(spawnPoints[randomSpawnPointIndex].transform.position,
                playerStats.transform.position);
            //Debug.Log("distanceToPlayer " + distanceToPlayer);

            if (distanceToPlayer <= 20f) {
                i--;
                continue;
            } else if (spawnPointsCounter.Contains(randomSpawnPointIndex) && currentRetriesToFindSpawnPoints < 10) {
                i--;
                continue;
            }

            // if (spawnPointsCounter.Contains(randomSpawnPointIndex)) {
            //     Debug.Log("spawn repeated");
            // }

            spawnPointsCounter.Add(randomSpawnPointIndex);
        }

        for (int i = 0; i < diffEnemiesToSpawn; i++) {
            // get a random enemy
            int randomEnemyIndex = Random.Range(0, enemiesQuantity);
            if (enemiesSpawnedCounter[randomEnemyIndex] >= enemiesToSpawnRatio) {
                i--;
                continue;
            }
            enemiesSpawnedCounter[randomEnemyIndex] += 1;
            // Debug.Log("randomEnemyIndex " + randomEnemyIndex);

            GameObject spawnPoint = spawnPoints[spawnPointsCounter[i]];

            // spawn the enemy
            Instantiate(
                enemiesPrefabs[randomEnemyIndex],
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );

            enemiesAlive++;
        }
    }
}
