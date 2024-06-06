using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Monster : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerStats playerStats;
    private Animator animator;
    private AudioSource enemyAudioSource;

    public int health = 100;
    public bool isDead = false;
    public int criticalHitDamage = 100;
    public int regularHitDamage = 25;
    public float defaultSpeed = 3.0f;
    public AudioClip tauntSound;

    private float timerToTaunt;
    private bool canWalk = true;
    private bool isRunning = false;
    private bool isAttacking = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyAudioSource = GetComponent<AudioSource>();

        playerStats = FindObjectOfType<PlayerStats>();

        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
    }

    void FixedUpdate() {
        if (playerStats.isDead) {
            StopWalking();
        }
        if (HudManager.Instance.IsRunningGame) {
            if (HudManager.Instance.IsPaused) {
                enemyAudioSource.Pause();
            } else {
                enemyAudioSource.UnPause();
            }
        }
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        if (canWalk) {
            agent.SetDestination(playerStats.transform.position);

            float distanceToPlayer = Vector3.Distance(
                transform.position,
                playerStats.transform.position);

            if (!isRunning && !isAttacking && distanceToPlayer <= 25f) {
                timerToTaunt += Time.deltaTime;
                if (timerToTaunt >= Random.Range(15, 55)) {
                    StopWalking();

                    animator.SetTrigger("Taunt");
                    enemyAudioSource.clip = tauntSound;
                    enemyAudioSource.Play();

                    StartCoroutine(WalkAfterSeconds(1));

                    timerToTaunt = 0;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player") && !playerStats.isDead && !isAttacking) {
            StopWalking();

            isAttacking = true;

            float attackTime = 1.5f;
            StartCoroutine(WalkAfterSeconds(attackTime));
            StartCoroutine(StopAttackAfterSeconds(attackTime));

            animator.SetTrigger("Attack");

            int damage = Random.Range(regularHitDamage - 10, regularHitDamage + 10);
            playerStats.ApplyDamage(damage);
            // Destroy(gameObject);
        }
    }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        StopWalking();

        animator.SetTrigger("Damage");

        float damageTime = 1f;

        // 50% chance of running or walking
        if (Random.Range(0, 2) == 0) {
            StartCoroutine(WalkAfterSeconds(damageTime));
        } else {
            StartCoroutine(RunAfterSeconds(damageTime));
            // after N seconds (from 4 to 10), start walking again
            int randSeconds = Random.Range(4, 11);
            StartCoroutine(WalkAfterSeconds(randSeconds));
        }

        if (health <= 0) {
            health = 0;
            isDead = true;
            MonsterManager.Instance.SpawnEnemiesDelayed();
            Destroy(gameObject);
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
    }
}
