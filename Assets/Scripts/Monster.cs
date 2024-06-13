using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    private float timerToTaunt;
    [SerializeField]
    public bool isBeingDamaged = false;
    [SerializeField]
    private bool canWalk = true;
    [SerializeField]
    private bool isRunning = false;
    [SerializeField]
    private bool isAttacking = false;
    [SerializeField]
    private float distanceToPlayer;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyAudioSource = GetComponent<AudioSource>();

        playerStats = FindObjectOfType<PlayerStats>();

        StartCoroutine(WalkAfterSeconds(0));
    }

    void FixedUpdate() {
        if (playerStats.isDead) {
            StopWalking();
        }

        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        agent.SetDestination(playerStats.transform.position);

        distanceToPlayer = Vector3.Distance(
            transform.position,
            playerStats.transform.position);
       
        if (distanceToPlayer <= distanceToAttack) {
            // the angle closer to 0 means it's looking at the player
            // float angleRotationDiff = Vector3.Angle(transform.forward, playerStats.transform.position - transform.position);

            // from -1 to 1 => -1 is looking at the opposite, 1 means it's looking at the player
            float dotRotationDiff = Vector3.Dot(transform.forward,
                (playerStats.transform.position - transform.position).normalized);

            if (dotRotationDiff > 0.8f && !playerStats.isDead && !isBeingDamaged && !isAttacking) {
                Attack();
            }
        } else {
            if (!playerStats.isDead && !isBeingDamaged && !isAttacking) {
                SetCanWalk(true);
            }
        }

        if (canWalk) {
            if (distanceToPlayer <= 10f && !isRunning && !isAttacking) {
                timerToTaunt += Time.deltaTime;
                if (timerToTaunt >= Random.Range(10, 30)) {
                    Taunt();
                    timerToTaunt = 0;
                }
            }
        }
    }

    private void Taunt() {
        StopWalking();
        animator.SetTrigger("Taunt");
        if (!enemyAudioSource.isPlaying || enemyAudioSource.clip != tauntSound) {
            enemyAudioSource.clip = tauntSound;
            enemyAudioSource.Play();
        }
        StartCoroutine(WalkAfterSeconds(1));
    }

    private void Attack() {
        StopWalking();

        isAttacking = true;
        animator.SetTrigger("Attack");

        StartCoroutine(WalkAfterSeconds(attackDurationTime * 1.5f));
        StartCoroutine(StopAttackAfterSeconds(attackDurationTime * 1.5f));

        int damage = Random.Range(regularHitDamage - damageVariation, regularHitDamage + damageVariation);
        playerStats.ApplyDamage(damage);
    }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        // stop walking and play damage animation
        StopWalking();
        animator.SetTrigger("Damage");
        isBeingDamaged = true;

        if (health <= 0) {
            health = 0;
            isDead = true;
            MonsterManager.Instance.SpawnEnemiesDelayed();
            Destroy(gameObject);
        }

        float recoveryTime = 1f;
        StartCoroutine(TurnBeingDamagedToFalseAfterSeconds(recoveryTime));
        // 50% chance of running or walking
        if (Random.Range(0, 2) == 0) {
            StartCoroutine(WalkAfterSeconds(recoveryTime));
        } else {
            StartCoroutine(RunAfterSeconds(recoveryTime));
            // run, and then after N seconds (from 4 to 10), start walking again
            int randSeconds = Random.Range(4, 11);
            StartCoroutine(WalkAfterSeconds(randSeconds));
        }
    }

    private void SetCanWalk(bool canWalk) {
        this.canWalk = canWalk;
        agent.isStopped = !canWalk;
    }

    public void StopWalking() {
        agent.speed = 0;
        SetCanWalk(false);
        isRunning = false;

        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
    }

    public IEnumerator WalkAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set can walk
        agent.speed = defaultSpeed;
        SetCanWalk(true);
        isRunning = false;

        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
    }

    public IEnumerator RunAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set can walk
        agent.speed = defaultSpeed * 2f;
        SetCanWalk(true);
        isRunning = true;

        animator.SetBool("Walking", false);
        animator.SetBool("Running", true);        
    }

    public IEnumerator StopAttackAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
        SetCanWalk(true);
    }

    public IEnumerator TurnBeingDamagedToFalseAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isBeingDamaged = false;
    }
}
