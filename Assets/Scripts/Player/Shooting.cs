using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

[RequireComponent(typeof(PlayerStats))]
public class Shooting : MonoBehaviour
{
    public GameObject particleShotEffect;
    public GameObject particleBloodEffect;
    public GameObject bulletHolePrefab;

    private PlayerStats playerStats;
    private float shootTimer = 0f;
    private float reloadTimer = 0f;

    void Awake() {
        playerStats = GetComponent<PlayerStats>();
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        Aim();

        if (CanManageGun() && Input.GetButtonDown("Reload") &&
            playerStats.spendAmmo &&
            playerStats.CurrentBullets != playerStats.MaxBullets &&
            playerStats.AvailableBullets > 0) {

            reloadTimer = 0;
            playerStats.isReloading = true;
        }

        ReloadGun();
    }

    private bool CanManageGun() {
        return !playerStats.isReloading && !playerStats.isBeingDamaged;
    }

    void Shoot() {
        if (CanManageGun() &&
            ((!playerStats.SelectedGun.isAutomaticGun && Input.GetButtonDown("Fire1")) ||
            (playerStats.SelectedGun.isAutomaticGun && Input.GetButton("Fire1")))
        ) {
            if (playerStats.CurrentBullets > 0) {
                Vector3 center = new(0.5F, 0.5F, 0);
                Ray ray = Camera.main.ViewportPointToRay(center);
                if (Physics.Raycast(ray, out RaycastHit bulletHit)) {
                    playerStats.GunAudioSource.PlayOneShot(playerStats.SelectedGun.gunShotSound);

                    playerStats.GunAnimator.Play("Shoot");

                    Transform gunBarrelShotTransform = playerStats.shotParticleEffectPos[playerStats.SelectedGunIndex].transform;

                    // shot fire effect particle
                    GameObject particleEffectInstance = Instantiate(
                        particleShotEffect,
                        gunBarrelShotTransform.position,
                        gunBarrelShotTransform.rotation
                    );
                    particleEffectInstance.transform.parent = playerStats.SelectedGunObject.transform;
                    Destroy(particleEffectInstance, 0.1f);

                    // bullet hole
                    if (bulletHit.transform.CompareTag(TagsController.Ground) ||
                        bulletHit.transform.CompareTag(TagsController.Ceiling) ||
                        bulletHit.transform.CompareTag(TagsController.Wall)) {
                        GameObject bulletHole = Instantiate(
                            bulletHolePrefab,
                            bulletHit.point + bulletHit.normal * 0.001f,
                            Quaternion.FromToRotation(Vector3.forward, bulletHit.normal)
                        );
                        Destroy(bulletHole, 10f);
                    }

                    // blood particle
                    if (bulletHit.transform.CompareTag(TagsController.EnemyHead) ||
                        bulletHit.transform.CompareTag(TagsController.Enemy)) {
                        GameObject bloodInstance = Instantiate(
                            particleBloodEffect,
                            bulletHit.point,
                            bulletHit.transform.rotation
                        );
                        Destroy(bloodInstance, 1f);
                    }

                    // damage
                    if (bulletHit.transform.CompareTag(TagsController.EnemyHead)) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        monster.ApplyDamage(playerStats.criticalHitDamage);
                    }

                    if (bulletHit.transform.CompareTag(TagsController.Enemy)) {
                        Monster monster = bulletHit.transform.GetComponent<Monster>();
                        int damage = Random.Range(playerStats.regularHitDamage - playerStats.damageVariation,
                            playerStats.regularHitDamage + playerStats.damageVariation);
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
                if (Input.GetButtonDown("Fire1")) {
                    playerStats.GunAudioSource.PlayOneShot(playerStats.SelectedGun.gunEmptySound);
                }
            }
        }
    }

    void Aim() {
        if (CanManageGun() && Input.GetButton("Fire2")) {
            shootTimer += Time.deltaTime;

            playerStats.EnterAimingState();

            if (shootTimer > playerStats.SelectedGun.timeToShootInterval) {
                playerStats.canShoot = true;
            }
            if (playerStats.canShoot) {
                Shoot();
            }

        } else {
            shootTimer = 0f;
            playerStats.ExitAimingState();
        }
    }

    void ReloadGun() {
        if (playerStats.isReloading) {
            if (!playerStats.GunAudioSource.isPlaying
                || playerStats.GunAudioSource.clip != playerStats.SelectedGun.gunReloadSound) {
                playerStats.GunAudioSource.clip = playerStats.SelectedGun.gunReloadSound;
                playerStats.GunAudioSource.Play();
            }

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

                playerStats.GunAudioSource.clip = null;
            }
        }
    }
}
