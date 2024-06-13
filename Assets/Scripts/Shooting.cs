using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class Shooting : MonoBehaviour
{
    public GameObject particleShotEffect;
    public GameObject particleBloodEffect;

    private PlayerStats playerStats;
    private float shootTimer = 0f;
    private float reloadTimer = 0f;

    public CinemachineVirtualCamera virtualCamera;
    public Camera gunsCamera;
    public int defaultFieldOfView = 50;

    void Awake() {
        playerStats = GetComponent<PlayerStats>();
    }

    void Update() {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || playerStats.isDead) {
            return;
        }

        Aim();

        if (CanManageGun() && Input.GetKeyDown(KeyCode.R) &&
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

                    /*
                    // bullet object
                    GameObject bulletInstance = Instantiate(
                        playerStats.SelectedGun.gunPrefabBullet,
                        gunBarrelShotTransform.position,
                        gunBarrelShotTransform.rotation
                    );
                    Physics.IgnoreCollision(bulletInstance.GetComponent<Collider>(), playerStats.GetComponentInChildren<CapsuleCollider>());
                    Physics.IgnoreCollision(bulletInstance.GetComponent<Collider>(), playerStats.GetComponentInChildren<SphereCollider>());
                    bulletInstance.GetComponent<Rigidbody>().AddForce(
                        gunBarrelShotTransform.forward * 20f,
                        ForceMode.Impulse
                    );
                    Destroy(bulletInstance, 5f);
                    */

                    // blood particle
                    if (bulletHit.transform.CompareTag("EnemyHead") || bulletHit.transform.CompareTag("Enemy")) {
                        GameObject bloodInstance = Instantiate(
                            particleBloodEffect,
                            bulletHit.point,
                            bulletHit.transform.rotation
                        );
                        Destroy(bloodInstance, 1f);
                    }

                    // damage
                    if (bulletHit.transform.CompareTag("EnemyHead")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        monster.ApplyDamage(playerStats.criticalHitDamage);
                    }

                    if (bulletHit.transform.CompareTag("Enemy")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
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
            playerStats.isAiming = true;

            playerStats.GunAnimator.SetBool("isReloading", false);
            playerStats.GunAnimator.SetBool("isAiming", true);

            virtualCamera.m_Lens.FieldOfView = 35;
            gunsCamera.fieldOfView = 35;

            if (shootTimer > playerStats.SelectedGun.timeToShootInterval) {
                playerStats.canShoot = true;
            }

            if (playerStats.canShoot) {
                Shoot();
            }

        } else {
            virtualCamera.m_Lens.FieldOfView = defaultFieldOfView;
            gunsCamera.fieldOfView = defaultFieldOfView;

            shootTimer = 0f;
            playerStats.isAiming = false;
            playerStats.canShoot = false;

            playerStats.GunAnimator.SetBool("isAiming", false);
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
