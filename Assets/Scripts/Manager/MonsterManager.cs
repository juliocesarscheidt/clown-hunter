using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    public int defaultRegularHitDamage = 25;
    public int regularHitDamage = 25;

    private int enemiesSpawnedCounter = 0;
    private Dictionary<int, Monster> monstersPool = new();

    public int maxSimultaneousAttacks = 2;
    // private readonly System.Threading.Lock attackLock = new();
    private readonly Object attackLock = new();
    [SerializeField]
    private List<int> releasedAttackToMonsterIds = new();

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

    public bool GetAttackLock(int monstedId) {
        if (releasedAttackToMonsterIds.Count < maxSimultaneousAttacks) {
            // only one monster can get this lock t a time, to avoid unexpected situations
            lock (attackLock) {
                releasedAttackToMonsterIds.Add(monstedId);
                return true;
            }
        }
        return false;
    }

    public void ReleaseAttackLock(int monstedId) {
        if (releasedAttackToMonsterIds.Count > 0) {
            lock (attackLock) {
                releasedAttackToMonsterIds.Remove(monstedId);
            }
        }
    }

    public void AddMonsterToPool(int id, Monster monster) {
        monstersPool.Add(id, monster);
    }

    public void RemoveMonsterFromPool(int id) {
        monstersPool.Remove(id);
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

        enemiesAlive = monstersPool.Keys.Count;
        Debug.Log($"enemies alive {enemiesAlive}");

        int enemiesDiffToSpawn = Mathf.Max(enemiesToSpawn - enemiesAlive, 0);
        Debug.Log($"enemies diff to spawn {enemiesDiffToSpawn}");
        if (enemiesDiffToSpawn == 0) {
            return;
        }

        List<int> randomSpawnPoints = new();

        int currentRetriesToFindSpawnPoints = 0;
        for (int i = 0; i < enemiesDiffToSpawn; i++) {
            // get a random spawn point, try to not get a repeated one
            int randomSpawnPointIndex = Random.Range(0, spawnPoints.Count);

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

        for (int i = 0; i < enemiesDiffToSpawn; i++) {
            GameObject spawnPoint = spawnPoints[randomSpawnPoints[i]];

            // spawn the enemy
            GameObject enemyObject = Instantiate(
                enemiesPrefabs[currentEnemySpawnedIndex],
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );
            enemyObject.name = $"{enemyObject.name}-{enemiesSpawnedCounter}"; 
            if (enemyObject.TryGetComponent(out Monster monster)) {
                monster.regularHitDamage = regularHitDamage;
                monster.monsterId = enemiesSpawnedCounter;
                AddMonsterToPool(enemiesSpawnedCounter, monster);
            }

            currentEnemySpawnedIndex++;
            if (currentEnemySpawnedIndex >= enemiesPrefabs.Count) {
                currentEnemySpawnedIndex = 0;
            }

            lastEnemySpawnedIndex = currentEnemySpawnedIndex;

            enemiesAlive++;
            enemiesSpawnedCounter++;
        }
    }
}
