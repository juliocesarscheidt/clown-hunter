using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Monster : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerStats playerStats;
    private Animator animator;
    private AudioSource monsterAudioSource;

    public int monsterId;

    public AudioClip tauntSound;
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
    private int probabilityToRun = 0;
    [SerializeField]
    private bool alreadyDraftAction = false;
    [SerializeField]
    private float timeToDraftProbability = 0f;
    [SerializeField]
    private float randomTimeToDraftAction = 0f;

    public bool isHittingOtherMonster = false;
    private Dictionary<int, Monster> hittingMonsterObjs = new();

    public bool isAttacking = false;
    private Coroutine setIsAttackingCoroutine;

    public bool takeDamage = true;
    public bool isBeingDamaged = false;
    private Coroutine setIsBeingDamagedCoroutine;

    public bool isTauting = false;
    private Coroutine setIsTautingCoroutine;
    private float timerToTaunt;

    private Vector3 targetPosition;
    [SerializeField]
    private float distanceToTarget;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        monsterAudioSource = GetComponent<AudioSource>();

        playerStats = FindObjectOfType<PlayerStats>();

        probabilityToRun = Random.Range(1, 21);
        randomTimeToDraftAction = Random.Range(5, 11);
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

        if (CanMove()) {
            agent.SetDestination(targetPosition);
            if (distanceToTarget > distanceToAttack && !isHittingOtherMonster) {
                if (alreadyDraftAction) {
                    // from 1 to 20
                    probabilityToRun = Random.Range(1, 21);
                    alreadyDraftAction = false;
                }
                // 10% of chance to run, otherwise walk
                if (probabilityToRun == 8 || probabilityToRun == 12) {
                    Run();
                } else {
                    Walk();
                }
            } else {
                StopWalk();
            }

            timeToDraftProbability += Time.deltaTime;
            // from 5 to 10 seconds
            if (!alreadyDraftAction && timeToDraftProbability >= randomTimeToDraftAction) {
                alreadyDraftAction = true;
                timeToDraftProbability = 0;
                randomTimeToDraftAction = Random.Range(5, 11);
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
                if (isInPointOfView && GetLock()) {
                    Attack();
                }
            } else {
                Quaternion targetRotation = Quaternion.LookRotation(playerStats.transform.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
            }
        }

        if (!isStopped) {
            if (distanceToTarget <= 10f && CanMove() && !isRunning) {
                timerToTaunt += Time.deltaTime;
                if (timerToTaunt >= Random.Range(10, 30)) {
                    Taunt();
                    timerToTaunt = 0;
                }
            }
        }
    }

    private bool CanMove() {
        return !playerStats.isDead && !isBeingDamaged && !isAttacking && !isTauting;
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

    private void Taunt() {
        isTauting = true;
        animator.SetTrigger("Taunt");
        if (!monsterAudioSource.isPlaying || monsterAudioSource.clip != tauntSound) {
            monsterAudioSource.clip = tauntSound;
            monsterAudioSource.Play();
        }

        if (setIsTautingCoroutine != null) StopCoroutine(setIsTautingCoroutine);
        setIsTautingCoroutine = StartCoroutine(SetIsTautingFalsyAfterSeconds(2f));
    }

    private void Attack() {
        isAttacking = true;
        animator.SetTrigger("Attack");

        if (setIsAttackingCoroutine != null) StopCoroutine(setIsAttackingCoroutine);
        setIsAttackingCoroutine = StartCoroutine(SetIsAttackingFalsyAfterSeconds(attackDurationTime * 1.5f));

        int damage = Random.Range(regularHitDamage - damageVariation, regularHitDamage + damageVariation);
        playerStats.ApplyDamage(damage);
    }

    public void ApplyDamage(int damage) {
        if (takeDamage) {
            health = Mathf.Max(health - damage, 0);
        }

        animator.SetTrigger("Damage");
        isBeingDamaged = true;

        if (health <= 0) {
            health = 0;
            isDead = true;
            
            ReleaseLock(); // release attack lock
            RemoveHittingFromAllOtherMonsters();

            MonsterManager.Instance.RemoveMonsterFromPool(monsterId);
            MonsterManager.Instance.SpawnEnemiesDelayed();

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

        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
    }

    private void Walk() {
        agent.speed = defaultSpeed;
        SetIsStopped(false);
        isRunning = false;

        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
    }

    private void Run() {
        agent.speed = defaultSpeed * 2f;
        SetIsStopped(false);
        isRunning = true;

        animator.SetBool("Walking", false);
        animator.SetBool("Running", true);
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

    public IEnumerator SetIsTautingFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isTauting = false;
    }

    private bool GetLock() {
        return MonsterManager.Instance.GetAttackLock(monsterId);
    }

    private void ReleaseLock() {
        // release the attack lock
        MonsterManager.Instance.ReleaseAttackLock(monsterId);
    }
}
