using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Globalization;
using System.Reflection;

public class Monster : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerStats playerStats;
    private Animator animator;
    private AudioSource enemyAudioSource;

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
    private float timerToRun;

    public bool isAttacking = false;
    private Coroutine setIsAttackingCoroutine;

    public bool isBeingDamaged = false;
    private Coroutine setIsBeingDamagedCoroutine;

    public bool isTauting = false;
    private Coroutine setIsTautingCoroutine;
    private float timerToTaunt;

    [SerializeField]
    private float distanceToPlayer;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyAudioSource = GetComponent<AudioSource>();

        playerStats = FindObjectOfType<PlayerStats>();
    }

    void FixedUpdate() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || isDead || playerStats.isDead) {
            StopWalk();
            return;
        }

        if (CanMove()) {
            agent.SetDestination(playerStats.transform.position);
            Walk();

        } else {
            StopWalk();
        }

        distanceToPlayer = Vector3.Distance(
            transform.position,
            playerStats.transform.position);
       
        if (distanceToPlayer <= distanceToAttack) {
            // from -1 to 1 => -1 is looking at the opposite, 1 means it's looking at the player
            float dotRotationDiff = Vector3.Dot(transform.forward,
                (playerStats.transform.position - transform.position).normalized);

            if (dotRotationDiff > 0.85f && CanMove()) {
                Attack();
            }
        }

        if (!isStopped) {
            if (distanceToPlayer <= 10f && CanMove()) {
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

    private void Taunt() {
        isTauting = true;
        animator.SetTrigger("Taunt");
        if (!enemyAudioSource.isPlaying || enemyAudioSource.clip != tauntSound) {
            enemyAudioSource.clip = tauntSound;
            enemyAudioSource.Play();
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
        health = Mathf.Max(health - damage, 0);

        animator.SetTrigger("Damage");
        isBeingDamaged = true;

        if (health <= 0) {
            health = 0;
            isDead = true;
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

    public IEnumerator WalkAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set to walking
        Walk();
    }

    public IEnumerator RunAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set to running
        Run();
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
    }

    public IEnumerator SetIsTautingFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isTauting = false;
    }
}
