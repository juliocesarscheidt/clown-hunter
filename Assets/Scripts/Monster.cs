using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class Monster : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;
    private PlayerStats playerStats;
    private Animator animator;

    public int health = 100;
    public bool isDead = false;
    public int criticalHitDamage = 100;
    public int regularHitDamage = 25;
    public float defaultSpeed = 3.0f;

    private float timerToTaunt;
    private bool canWalk = true;
    private bool isRunning = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();

        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
    }

    void FixedUpdate() {
        if (playerStats.isDead) {
            StopWalking();
            return;
        }
        if (HudManager.Instance.IsPaused()) {
            return;
        }

        if (canWalk) {
            agent.SetDestination(player.transform.position);

            if (!isRunning) {
                timerToTaunt += Time.deltaTime;
                if (timerToTaunt >= UnityEngine.Random.Range(15, 45)) {
                    StopWalking();
                    StartCoroutine(WalkAfterSeconds(1));

                    animator.SetTrigger("Taunt");
                    timerToTaunt = 0;
                }
            } else {
                timerToTaunt = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            StopWalking();

            int hitTime = 2;
            StartCoroutine(WalkAfterSeconds(hitTime));

            animator.SetTrigger("Attack");

            playerStats.ApplyDamage(regularHitDamage);
            // Destroy(gameObject);
        }
    }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        StopWalking();

        animator.SetTrigger("Hit");

        int damageTime = 1;

        // 50% chance of running or walking
        if (UnityEngine.Random.Range(0, 2) == 0) {
            StartCoroutine(WalkAfterSeconds(damageTime));
        } else {
            StartCoroutine(RunAfterSeconds(damageTime));
            // after N seconds, start walking again
            int randSeconds = UnityEngine.Random.Range(4, 9);
            StartCoroutine(WalkAfterSeconds(randSeconds));
        }

        if (health <= 0) {
            health = 0;
            isDead = true;
            Destroy(gameObject);
        }
    }

    public void StopWalking() {
        agent.speed = 0;

        canWalk = false;
        agent.isStopped = !canWalk;
        isRunning = false;

        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
    }

    public IEnumerator WalkAfterSeconds(int seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set can walk
        agent.speed = defaultSpeed;

        canWalk = true;
        agent.isStopped = !canWalk;
        isRunning = false;

        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
    }

    public IEnumerator RunAfterSeconds(int seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        // set can walk
        agent.speed = defaultSpeed * 2f;

        canWalk = true;
        agent.isStopped = !canWalk;
        isRunning = true;

        animator.SetBool("Walking", false);
        animator.SetBool("Running", true);        
    }
}
