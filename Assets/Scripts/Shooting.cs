using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class Shooting : MonoBehaviour
{
    public GameObject[] gunObject;
    public GameObject particleShotEffect;
    public GameObject[] shotEffectPos;

    private AudioSource gunAudioSource;
    private Animator gunAnimator;
    private PlayerStats playerStats;

    public AudioClip[] gunShotSound;
    public AudioClip[] gunReloadSound;
    public AudioClip[] gunEmptySound;

    private int selectedGun = 0;

    public float[] timeToShootInterval; // new float[] { 0.3f, 0.05f }
    private float shootTimer = 0f;
    private bool canShoot = false;

    public float timeToReloadInterval = 3f;
    private float reloadTimer = 0f;
    private bool isReloading = false;

    private bool isAutomaticGun = false;

    void Start() {
        playerStats = GetComponent<PlayerStats>();

        SetCurrentGun();
    }

    void Update() {
        if (HudManager.Instance.IsPaused() || playerStats.isDead) {
            return;
        }

        Aim();

        if (Input.GetKeyDown(KeyCode.R) && !isReloading &&
            playerStats.currentBullets[selectedGun] != playerStats.maxBullets[selectedGun] &&
            playerStats.availableBullets[selectedGun] > 0) {

            gunAudioSource.PlayOneShot(gunReloadSound[selectedGun]);

            reloadTimer = 0;
            isReloading = true;
        }

        ReloadGun();
        ChangeGun();
    }

    void Shoot() {
        if (!isReloading &&
            ((!isAutomaticGun && Input.GetMouseButtonDown(0)) ||
            (isAutomaticGun && Input.GetMouseButton(0)))
        ) {
            if (playerStats.currentBullets[selectedGun] > 0) {
                if (Physics.Raycast(
                    Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit bulletHit)
                ) {
                    gunAudioSource.PlayOneShot(gunShotSound[selectedGun]);

                    gunAnimator.Play("Shoot");
                    // gunAnimator.SetTrigger("Shoot");

                    GameObject particleInstance = Instantiate(
                        particleShotEffect,
                        shotEffectPos[selectedGun].transform.position,
                        shotEffectPos[selectedGun].transform.rotation
                    );
                    particleInstance.transform.parent = gunObject[selectedGun].transform;
                    Destroy(particleInstance, 0.1f);

                    // damage
                    if (bulletHit.transform.CompareTag("EnemyHead")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        monster.ApplyDamage(playerStats.criticalHitDamage);
                    }

                    if (bulletHit.transform.CompareTag("Enemy")) {
                        Monster monster = bulletHit.transform.GetComponentInParent<Monster>();
                        monster.ApplyDamage(playerStats.regularHitDamage);
                    }

                    // shoot control
                    canShoot = false;
                    shootTimer = 0f;

                    playerStats.currentBullets[selectedGun]--;
                    HudManager.Instance.AdjustBulletsCount();
                }

            // no bullets
            } else {
                gunAudioSource.PlayOneShot(gunEmptySound[selectedGun]);
            }
        }
    }

    void Aim() {
        if (!isReloading && Input.GetMouseButton(1)) {
            shootTimer += Time.deltaTime;

            gunAnimator.SetBool("isReloading", false);
            gunAnimator.SetBool("isAiming", true);

            if (shootTimer > timeToShootInterval[selectedGun]) {
                canShoot = true;
            }

            if (canShoot) {
                Shoot();
            }

        } else {
            canShoot = false;
            shootTimer = 0f;

            gunAnimator.SetBool("isAiming", false);
        }
    }

    void ReloadGun() {
        if (isReloading) {
            gunAnimator.SetBool("isAiming", false);
            gunAnimator.SetBool("isReloading", true);

            reloadTimer += Time.deltaTime;

            if (reloadTimer > timeToReloadInterval) {
                int diffBullets = Mathf.Min(
                    playerStats.maxBullets[selectedGun] - playerStats.currentBullets[selectedGun],
                    playerStats.availableBullets[selectedGun]
                 );
                playerStats.availableBullets[selectedGun] -= diffBullets;
                playerStats.currentBullets[selectedGun] += diffBullets;

                HudManager.Instance.AdjustBulletsCount();

                reloadTimer = 0;
                isReloading = false;
                gunAnimator.SetBool("isReloading", false);
            }
        }
    }

    void ChangeGun() {
        if (isReloading) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            selectedGun = 0;
            SetCurrentGun();

        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            selectedGun = 1;
            SetCurrentGun();
        }

        HudManager.Instance.AdjustBulletsCount();
    }

    void SetCurrentGun() {
        foreach (var gun in gunObject) {
            gun.SetActive(false);
        }
        gunObject[selectedGun].SetActive(true);
        gunAnimator = gunObject[selectedGun].GetComponent<Animator>();
        gunAudioSource = gunObject[selectedGun].GetComponent<AudioSource>();

        if (selectedGun == 0) {
            isAutomaticGun = false;
        } else if (selectedGun == 1) {
            isAutomaticGun = true;
        }
    }

    public int SelectedGun() {
        return selectedGun;
    }
}
