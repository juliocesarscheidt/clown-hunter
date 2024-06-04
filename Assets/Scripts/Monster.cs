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
    private bool isAttacking = false;

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
        }
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }

        if (canWalk) {
            agent.SetDestination(player.transform.position);

            if (!isRunning && !isAttacking) {
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
        if (collision.gameObject.CompareTag("Player") && !playerStats.isDead && !isAttacking) {
            StopWalking();

            isAttacking = true;

            float attackTime = 1.5f;
            StartCoroutine(WalkAfterSeconds(attackTime));
            StartCoroutine(StopAttackAfterSeconds(attackTime));

            animator.SetTrigger("Attack");

            playerStats.ApplyDamage(regularHitDamage);
            // Destroy(gameObject);
        }
    }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        StopWalking();

        animator.SetTrigger("Damage");

        float damageTime = 1f;

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
