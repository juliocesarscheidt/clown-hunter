using StarterAssets;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public bool isDead = false;

    public int criticalHitDamage = 100;
    public int regularHitDamage = 50;

    public int[] currentBullets; // new int[] { 6, 30 }
    public int[] maxBullets; // new int[] { 6, 30 }
    public int[] availableBullets; // new int[] { 12, 60 }

    private FirstPersonController playerController;

    public AudioSource stepsAudioSource;
    public AudioClip stepsWalkingAudioClip;
    public AudioClip stepsRunningAudioClip;

    public Animator cameraAnimator;

    void Start() {
        playerController = GetComponent<FirstPersonController>();
    }

    public void PlayerWalk(bool isWalking) {
        if (HudManager.Instance.IsPaused() || isDead) {
            return;
        }
        if (isWalking) {
            if (!stepsAudioSource.isPlaying
                || stepsAudioSource.isPlaying && stepsAudioSource.clip != stepsWalkingAudioClip) {
                stepsAudioSource.clip = stepsWalkingAudioClip;
                stepsAudioSource.Play();
            }
        } else {
            stepsAudioSource.Stop();
        }
    }

    public void PlayerRun(bool isRunning) {
        if (HudManager.Instance.IsPaused() || isDead) {
            return;
        }
        if (isRunning) {
            if (!stepsAudioSource.isPlaying
                || stepsAudioSource.isPlaying && stepsAudioSource.clip != stepsRunningAudioClip) {
                stepsAudioSource.clip = stepsRunningAudioClip;
                stepsAudioSource.Play();
            }
        } else {
            stepsAudioSource.Stop();
        }
    }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        playerController.CanMovePlayer = false;
        StartCoroutine(EnablePlayerMovementAfterSeconds(1.5f));

        cameraAnimator.SetTrigger("Damage");

        HudManager.Instance.AdjustHealthBar(health, 100);
        HudManager.Instance.ShowBloodImage();

        if (health <= 0) {
            health = 0;
            isDead = true;
            HudManager.Instance.ShowGameOverImage();
        }
    }

    public void CollectAmmo() {
        for (int i = 0; i < availableBullets.Length; i++) {
            availableBullets[i] += maxBullets[i];
        }
    }

    private IEnumerator EnablePlayerMovementAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        playerController.CanMovePlayer = true;
    }

    public void DisablePlayerMovementAndCamera() {
        // playerInput.enabled = false;
        playerController.CanMovePlayer = false;
        playerController.CanMoveCamera = false;
    }

    public void EnablePlayerMovementAndCamera() {
        // playerInput.enabled = true;
        playerController.CanMovePlayer = true;
        playerController.CanMoveCamera = true;
    }

}
