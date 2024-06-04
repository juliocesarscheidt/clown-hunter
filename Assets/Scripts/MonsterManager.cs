using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    private GameObject player;
    private PlayerStats playerStats;

    public List<GameObject> spawnPoints;
    public GameObject enemyPrefab;

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

        int spawnPointsQuantity = spawnPoints.Count;
        int diffEnemiesToSpawn = Mathf.Max(enemiesToSpawn - enemiesAlive, 0);

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
            GameObject spawnPoint = spawnPoints[spawnPointsCounter[i]];
            Instantiate(
                enemyPrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );
            enemiesAlive++;
        }
    }
}
