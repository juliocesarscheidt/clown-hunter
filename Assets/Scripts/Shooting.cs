using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class Shooting : MonoBehaviour
{
    public GameObject particleShotEffect;
    private PlayerStats playerStats;
    private float shootTimer = 0f;
    private float reloadTimer = 0f;

    void Start() {
        playerStats = GetComponent<PlayerStats>();
    }

    void Update() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }

        Aim();
        
        if (Input.GetKeyDown(KeyCode.R) && !playerStats.isReloading &&
            playerStats.CurrentBullets != playerStats.MaxBullets &&
            playerStats.AvailableBullets > 0) {

            playerStats.GunAudioSource.PlayOneShot(playerStats.SelectedGun.gunReloadSound);

            reloadTimer = 0;
            playerStats.isReloading = true;
        }

        ReloadGun();
    }

    void Shoot() {
        if (!playerStats.isReloading &&
            ((!playerStats.SelectedGun.isAutomaticGun && Input.GetMouseButtonDown(0)) ||
            (playerStats.SelectedGun.isAutomaticGun && Input.GetMouseButton(0)))
        ) {
            if (playerStats.CurrentBullets > 0) {
                if (Physics.Raycast(
                    Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit bulletHit)
                ) {
                    playerStats.GunAudioSource.PlayOneShot(playerStats.SelectedGun.gunShotSound);

                    playerStats.GunAnimator.Play("Shoot");
                    // gunAnimator.SetTrigger("Shoot");

                    GameObject particleInstance = Instantiate(
                        particleShotEffect,
                        playerStats.shotParticleEffectPos[playerStats.SelectedGunIndex].transform.position,
                        playerStats.shotParticleEffectPos[playerStats.SelectedGunIndex].transform.rotation
                    );
                    particleInstance.transform.parent = playerStats.SelectedGunObject.transform;
                    Destroy(particleInstance, 0.1f);

                    // damage
                    if (bulletHit.transform.CompareTag("EnemyHead")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        monster.ApplyDamage(playerStats.criticalHitDamage);
                    }

                    if (bulletHit.transform.CompareTag("Enemy")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        int damage = Random.Range(playerStats.regularHitDamage - 10, playerStats.regularHitDamage + 10);
                        monster.ApplyDamage(damage);
                    }

                    // shoot control
                    playerStats.canShoot = false;
                    shootTimer = 0f;

                    playerStats.CurrentBullets--;
                    HudManager.Instance.AdjustBulletsCount();
                }

                // no bullets
            } else {
                // make sure it's triggered by the click each time
                if (Input.GetMouseButtonDown(0)) {
                    playerStats.GunAudioSource.PlayOneShot(playerStats.SelectedGun.gunEmptySound);
                }
            }
        }
    }

    void Aim() {
        if (!playerStats.isReloading && Input.GetMouseButton(1)) {
            shootTimer += Time.deltaTime;
            playerStats.isAiming = true;

            playerStats.GunAnimator.SetBool("isReloading", false);
            playerStats.GunAnimator.SetBool("isAiming", true);

            if (shootTimer > playerStats.SelectedGun.timeToShootInterval) {
                playerStats.canShoot = true;
            }

            if (playerStats.canShoot) {
                Shoot();
            }

        } else {
            shootTimer = 0f;
            playerStats.isAiming = false;
            playerStats.canShoot = false;

            playerStats.GunAnimator.SetBool("isAiming", false);
        }
    }

    void ReloadGun() {
        if (playerStats.isReloading) {
            playerStats.GunAnimator.SetBool("isAiming", false);
            playerStats.GunAnimator.SetBool("isReloading", true);

            reloadTimer += Time.deltaTime;

            if (reloadTimer > playerStats.SelectedGun.timeToReloadInterval) {
                int diffBullets = Mathf.Min(
                    playerStats.MaxBullets - playerStats.CurrentBullets,
                    playerStats.AvailableBullets
                );
                playerStats.AvailableBullets -= diffBullets;
                playerStats.CurrentBullets += diffBullets;

                HudManager.Instance.AdjustBulletsCount();

                reloadTimer = 0;
                playerStats.isReloading = false;
                playerStats.GunAnimator.SetBool("isReloading", false);
            }
        }
    }
}
