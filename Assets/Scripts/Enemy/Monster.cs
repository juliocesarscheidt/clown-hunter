using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Monster : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerStats playerStats;
    private Animator animator;
    private AudioSource monsterAudioSource;

    public int monsterId;

    public AudioClip laughSound;
    public int health = 100;
    public float defaultSpeed = 3.0f;
    public bool isDead = false;
    public int criticalHitDamage = 100;
    public int regularHitDamage = 25;
    public int damageVariation = 10;
    public float distanceToAttack = 1.25f;
    public float attackDurationTime = 1.5f;

    public bool isStopped = false;

    public bool isRunning = false;
    [SerializeField]
    private int currentActionRaffledNumber = 0;
    [SerializeField]
    private bool alreadyRaffledAction = false;
    [SerializeField]
    private float timerToRaffleAction = 0f;
    [SerializeField]
    private float randomPeriodToRaffleAction = 0f;

    public readonly int maxProbabilityNumberToRaffle = 100;
    public readonly int minProbabilityNumberToRaffle = 1;

    public float runProbabilityPercentage = 15f;
    [SerializeField]
    private List<int> probabilityToRunRaffledNumbers = new();

    private enum States {
        WALKING,
        RUNNING,
        IDLE,
        ATTACKING,
        LAUGHING,
        TAKING_DAMAGE,
    };
    [SerializeField]
    private States currentState = States.IDLE;
    public TMP_Text currentStateText;
    public bool showCurrentState = false;

    public bool isHittingOtherMonster = false;
    private Dictionary<int, Monster> hittingMonsterObjs = new();

    public bool isAttacking = false;
    private Coroutine setIsAttackingCoroutine;

    public bool canReceiveDamage = true;
    public bool isBeingDamaged = false;
    private Coroutine setIsBeingDamagedCoroutine;

    public bool isLaughing = false;
    private Coroutine setIsLaughingCoroutine;
    private float timerToLaugh;

    private Vector3 targetPosition;
    [SerializeField]
    private float distanceToTarget;

    private readonly Dictionary<string, int> animationHashes = new() {
        { "Walking", Animator.StringToHash("Walking") },
        { "Running", Animator.StringToHash("Running") },
        { "Attack", Animator.StringToHash("Attack") },
        { "Damage", Animator.StringToHash("Damage") },
        { "Laugh", Animator.StringToHash("Laugh") },
    };

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        monsterAudioSource = GetComponent<AudioSource>();

        playerStats = FindObjectOfType<PlayerStats>();

        FillProbabilityToRunRaffledNumbers();
        RaffleActionRaffledNumber();
        RaffleRandomPeriodToRaffleAction();
    }

    void FixedUpdate() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || isDead || playerStats.isDead) {
            StopWalk();
            return;
        }

        targetPosition = playerStats.pointToMonsterAttack.transform.position;
        distanceToTarget = Vector3.Distance(
            transform.position,
            targetPosition
        );

        if (showCurrentState) {
            currentStateText.gameObject.SetActive(true);
            currentStateText.text = currentState.ToString();
        } else {
            currentStateText.gameObject.SetActive(false);
        }

        if (CanMove()) {
            agent.SetDestination(targetPosition);

            if (distanceToTarget > distanceToAttack && !isHittingOtherMonster) {
                if (!alreadyRaffledAction) {
                    RaffleActionRaffledNumber();
                }
                // 10% of chance to run, otherwise walk
                if (probabilityToRunRaffledNumbers.Contains(currentActionRaffledNumber)) {
                    Run();
                } else {
                    Walk();
                }
            } else {
                StopWalk();
            }

            timerToRaffleAction += Time.deltaTime;
            if (alreadyRaffledAction && timerToRaffleAction >= randomPeriodToRaffleAction) {
                alreadyRaffledAction = false;
                timerToRaffleAction = 0;
                RaffleRandomPeriodToRaffleAction();
            }
        } else {
            StopWalk();
        }
       
        if (distanceToTarget <= distanceToAttack && CanMove()) {
            // from -1 to 1 => -1 is looking at the opposite, 1 means it's looking at the player
            float dotRotationMonsterToPlayerDiff = Vector3.Dot(transform.forward,
                (playerStats.transform.position - transform.position).normalized);

            if (dotRotationMonsterToPlayerDiff > 0.85f) {
                // calling this inside this block to avoid unneeded calls
                bool isInPointOfView = playerStats.ObjectIsInPointOfView(gameObject);
                if (isInPointOfView && GetAttackLock()) {
                    Attack();
                }
            } else {
                Quaternion targetRotation = Quaternion.LookRotation(playerStats.transform.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
            }
        }

        if (!isStopped) {
            if (distanceToTarget <= 10f && CanMove() && !isRunning) {
                timerToLaugh += Time.deltaTime;
                // from 10 to 35 seconds
                if (timerToLaugh >= Random.Range(10, 36)) {
                    Laugh();
                    timerToLaugh = 0;
                }
            }
        }
    }

    private bool CanMove() {
        return !playerStats.isDead && !isBeingDamaged && !isAttacking && !isLaughing;
    }

    public void ChangeRunProbabilityPercentage(float percentage) {
        if (percentage < minProbabilityNumberToRaffle || percentage > maxProbabilityNumberToRaffle) return;
        runProbabilityPercentage = percentage;
        FillProbabilityToRunRaffledNumbers();
    }

    private void FillProbabilityToRunRaffledNumbers() {
        probabilityToRunRaffledNumbers.Clear();

        int amountOfNumbersToRaffleForRunProbability = Mathf.FloorToInt(maxProbabilityNumberToRaffle * (runProbabilityPercentage / 100f));
        int halfOfNumbersToRaffle = Mathf.FloorToInt(amountOfNumbersToRaffleForRunProbability / 2f);
        int halfOfProbabilitiesNumber = maxProbabilityNumberToRaffle / 2;

        for (int i = halfOfProbabilitiesNumber - halfOfNumbersToRaffle; i < halfOfProbabilitiesNumber + halfOfNumbersToRaffle + 1; i++) {
            if (i < minProbabilityNumberToRaffle || i > maxProbabilityNumberToRaffle) continue;
            if (probabilityToRunRaffledNumbers.Count >= amountOfNumbersToRaffleForRunProbability) break;
            probabilityToRunRaffledNumbers.Add(i);
        }
    }

    private void RaffleRandomPeriodToRaffleAction() {
        // from 5 to 15 seconds
        randomPeriodToRaffleAction = Random.Range(5, 16);
    }

    private void RaffleActionRaffledNumber() {
        // from 1 to 100 numbers by default
        currentActionRaffledNumber = Random.Range(minProbabilityNumberToRaffle, maxProbabilityNumberToRaffle + 1);
        alreadyRaffledAction = true;
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.CompareTag(TagsController.EnemyArea)) {
            Monster other = collider.gameObject.GetComponentInParent<Monster>();
            if (other != null) {
                if (!hittingMonsterObjs.ContainsKey(other.monsterId)) {
                    hittingMonsterObjs.Add(other.monsterId, other);
                }
                if (!other.hittingMonsterObjs.ContainsKey(monsterId)) {
                    other.hittingMonsterObjs.Add(monsterId, this);
                }

                // check which monster is closer to player, that one will continue walking, but the other will stop
                if (other.distanceToTarget > distanceToTarget) {
                    other.isHittingOtherMonster = true;
                    isHittingOtherMonster = false;
                } else {
                    isHittingOtherMonster = true;
                    other.isHittingOtherMonster = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.gameObject.CompareTag(TagsController.EnemyArea)) {
            Monster other = collider.gameObject.GetComponentInParent<Monster>();
            if (other != null) {
                RemoveHittingOtherMonster(other.monsterId);
                hittingMonsterObjs.Remove(other.monsterId);
            }
            isHittingOtherMonster = false;
        }
    }

    private void RemoveHittingOtherMonster(int otherMonsterId) {
        if(!hittingMonsterObjs.ContainsKey(otherMonsterId)) {
            return;
        }
        var monster = hittingMonsterObjs[otherMonsterId];
        if (monster != null) {
            monster.hittingMonsterObjs.Remove(monsterId);
            monster.isHittingOtherMonster = false;
        }
    }

    private void RemoveHittingFromAllOtherMonsters() {
        foreach (var otherMonsterId in hittingMonsterObjs.Keys) {
            RemoveHittingOtherMonster(otherMonsterId);
        }
        isHittingOtherMonster = false;
    }

    private void Laugh() {
        isLaughing = true;

        animator.SetTrigger(animationHashes["Laugh"]);
        if (!monsterAudioSource.isPlaying || monsterAudioSource.clip != laughSound) {
            monsterAudioSource.clip = laughSound;
            monsterAudioSource.Play();
        }

        if (setIsLaughingCoroutine != null) StopCoroutine(setIsLaughingCoroutine);
        setIsLaughingCoroutine = StartCoroutine(SetIsLaughingFalsyAfterSeconds(3f));

        currentState = States.LAUGHING;
    }

    private void Attack() {
        isAttacking = true;
        animator.SetTrigger(animationHashes["Attack"]);

        if (setIsAttackingCoroutine != null) StopCoroutine(setIsAttackingCoroutine);
        setIsAttackingCoroutine = StartCoroutine(SetIsAttackingFalsyAfterSeconds(attackDurationTime * 1.5f));

        int damage = Random.Range(regularHitDamage - damageVariation, regularHitDamage + damageVariation);
        playerStats.ApplyDamage(damage);

        currentState = States.ATTACKING;
    }

    public void ApplyDamage(int damage) {
        if (canReceiveDamage) {
            health = Mathf.Max(health - damage, 0);
        }

        animator.SetTrigger(animationHashes["Damage"]);
        isBeingDamaged = true;

        currentState = States.TAKING_DAMAGE;

        if (health <= 0) {
            health = 0;
            isDead = true;
            
            ReleaseLock(); // release attack lock
            RemoveHittingFromAllOtherMonsters();

            MonsterManager.Instance.RemoveMonsterFromPool(monsterId);
            MonsterManager.Instance.AddMonsterDeadToTaskManager();
            MonsterManager.Instance.SpawnEnemies();

            Destroy(gameObject);
        }

        float recoveryTime = 1f;
        if (setIsBeingDamagedCoroutine != null) StopCoroutine(setIsBeingDamagedCoroutine);
        setIsBeingDamagedCoroutine = StartCoroutine(SetIsBeingDamagedFalsyAfterSeconds(recoveryTime));
    }

    private void SetIsStopped(bool isStopped) {
        this.isStopped = isStopped;
        agent.isStopped = isStopped;
    }

    public void StopWalk() {
        agent.speed = 0;
        SetIsStopped(true);
        isRunning = false;

        animator.SetBool(animationHashes["Walking"], false);
        animator.SetBool(animationHashes["Running"], false);

        if (!isBeingDamaged && !isAttacking && !isLaughing) {
            currentState = States.IDLE;
        }
    }

    private void Walk() {
        agent.speed = defaultSpeed;
        SetIsStopped(false);
        isRunning = false;

        animator.SetBool(animationHashes["Running"], false);
        animator.SetBool(animationHashes["Walking"], true);

        currentState = States.WALKING;
    }

    private void Run() {
        agent.speed = defaultSpeed * 2f;
        SetIsStopped(false);
        isRunning = true;

        animator.SetBool(animationHashes["Walking"], false);
        animator.SetBool(animationHashes["Running"], true);

        currentState = States.RUNNING;
    }

    public IEnumerator SetIsBeingDamagedFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isBeingDamaged = false;
    }

    public IEnumerator SetIsAttackingFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
        ReleaseLock();
    }

    public IEnumerator SetIsLaughingFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isLaughing = false;
    }

    private bool GetAttackLock() {
        return MonsterManager.Instance.GetAttackLock(monsterId);
    }

    private void ReleaseLock() {
        // release the attack lock
        MonsterManager.Instance.ReleaseAttackLock(monsterId);
    }
}
