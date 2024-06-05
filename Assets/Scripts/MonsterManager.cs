using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    private GameObject player;
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
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();
    }

    void FixedUpdate() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }
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

        for (int i = 0; i < diffEnemiesToSpawn; i++) {
            // get a random spawn point
            int randomSpawnPointIndex = Random.Range(0, spawnPointsQuantity);
            if (spawnPointsCounter.Contains(randomSpawnPointIndex)) {
                i--;
                continue;
            }
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
