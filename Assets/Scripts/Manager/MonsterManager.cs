using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TaskManager;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    private PlayerStats playerStats;

    public List<GameObject> spawnPoints;
    public List<GameObject> enemiesPrefabs;

    public int monstersToSpawn = 4;
    private int monstersAlive = 0;
    private int lastEnemySpawnedIndex = 0;

    private int defaultRegularHitDamage = 25;
    public int regularHitDamage = 25;

    private int enemiesSpawnedCounter = 0;
    private Dictionary<int, Monster> monstersPool = new();

    public int maxSimultaneousAttacks = 2;
    private readonly Object attackLock = new();
    [SerializeField]
    private List<int> releasedAttackToMonsterIds = new();

    public float runProbabilityPercentage = 15f;
    public bool canReceiveDamage = true;
    public bool showCurrentState = false;

    private int thisTaskIndex = (int) TaskType.EliminateTheRemainingClowns;

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

    public void ChangeRunProbabilityPercentageToAllMonsters(float percentage) {
        runProbabilityPercentage = percentage;
        foreach (Monster monster in monstersPool.Values) {
            if (monster != null) {
                monster.ChangeRunProbabilityPercentage(runProbabilityPercentage);
            }
        }
    }

    public void ChangeRegularHitDamageToAllMonsters(int addHitDamageAmount) {
        regularHitDamage = defaultRegularHitDamage + addHitDamageAmount;
        foreach (Monster monster in monstersPool.Values) {
            if (monster != null) {
                monster.regularHitDamage = regularHitDamage;
            }
        }
    }

    public void ChangeCanReceiveDamageToAllMonsters(bool canReceive) {
        canReceiveDamage = canReceive;
        foreach (Monster monster in monstersPool.Values) {
            if (monster != null) {
                monster.canReceiveDamage = canReceiveDamage;
            }
        }
    }

    public void ChangeShowCurrentStateToAllMonsters(bool show) {
        showCurrentState = show;
        foreach (Monster monster in monstersPool.Values) {
            if (monster != null) {
                monster.showCurrentState = showCurrentState;
            }
        }
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
        UpdateMonstersAliveCounter();
    }

    public void RemoveMonsterFromPool(int id) {
        monstersPool.Remove(id);
        UpdateMonstersAliveCounter();
    }

    public void AddMonsterDeadToTaskManager() {
        TaskManager.Instance.UpdateTaskProgress(thisTaskIndex, +1);
    }

    public IEnumerator SpawnEnemiesAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        SpawnEnemies();
    }

    public void SpawnEnemiesDelayed() {
        StartCoroutine(SpawnEnemiesAfterSeconds(2f));
    }

    private void UpdateMonstersAliveCounter() {
        monstersAlive = monstersPool.Keys.Count;
        // Debug.Log($"monsters alive {monstersAlive}");
        TaskManager.Instance.UpdateTaskTotalProgress(thisTaskIndex, monstersAlive);
    }

    public void SpawnEnemies() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        // update counter initially to check how many monsters to spawn
        UpdateMonstersAliveCounter();

        int enemiesDiffToSpawn = Mathf.Max(monstersToSpawn - monstersAlive, 0);
        // Debug.Log($"enemies diff to spawn {enemiesDiffToSpawn}");
        if (enemiesDiffToSpawn == 0) {
            TaskManager.Instance.UpdateTaskTotalProgress(thisTaskIndex, monstersAlive);
            return;
        }
        
        // if is the current task, stop spawning more monsters
        if (TaskManager.Instance.IsCurrentTask(thisTaskIndex)) {
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
                monster.monsterId = enemiesSpawnedCounter;
                // setting configurations to monster
                monster.regularHitDamage = regularHitDamage;
                monster.canReceiveDamage = canReceiveDamage;
                monster.showCurrentState = showCurrentState;
                monster.ChangeRunProbabilityPercentage(runProbabilityPercentage);

                AddMonsterToPool(enemiesSpawnedCounter, monster);
            }

            currentEnemySpawnedIndex++;
            if (currentEnemySpawnedIndex >= enemiesPrefabs.Count) {
                currentEnemySpawnedIndex = 0;
            }

            lastEnemySpawnedIndex = currentEnemySpawnedIndex;

            enemiesSpawnedCounter++;
        }

        // update counter again after spawning
        UpdateMonstersAliveCounter();
    }
}
