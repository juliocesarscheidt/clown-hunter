using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public bool isDead = false;

    public int criticalHitDamage = 100;
    public int regularHitDamage = 50;

    private FirstPersonController playerController;

    public AudioSource stepsAudioSource;
    public AudioClip stepsWalkingAudioClip;
    public AudioClip stepsRunningAudioClip;
    public Animator cameraAnimator;

    public bool canShoot = false;
    public bool isReloading = false;
    public bool isAiming = false;
    public bool isBeingDamaged = false;

    public GameObject[] shotParticleEffectPos;

    private Animator gunAnimator;
    private AudioSource gunAudioSource;

    public GameObject gunsGameObjectParent;
    public List<Weapon> guns;
    [SerializeField]
    private Weapon selectedGun;
    [SerializeField]
    private int selectedGunIndex = 0;
    [SerializeField]
    private GameObject selectedGunObject;

    [SerializeField]
    private List<int> currentBullets = new();
    [SerializeField]
    private List<int> maxBullets = new();
    [SerializeField]
    private List<int> availableBullets = new();

    void Awake() {
        for (int i = 0; i < guns.Count; i++) {
            currentBullets.Add(guns[i].currentBullets);
            maxBullets.Add(guns[i].maxBullets);
            availableBullets.Add(guns[i].availableBullets);
        }
        playerController = GetComponent<FirstPersonController>();
    }

    void Start() {
        SelectGun(0);
    }

    void Update() {
        if (HudManager.Instance.IsRunningGame) {
            if (HudManager.Instance.IsPaused) {
                GunAudioSource.Pause();
                stepsAudioSource.Pause();
            } else {
                GunAudioSource.UnPause();
                stepsAudioSource.UnPause();
            }
        }

        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || isDead) {
            return;
        }

        if (!isReloading) {
            Enumerable.Range(1, guns.Count).ToList().ForEach(idx => {
                if (Input.GetKeyDown(idx.ToString())) {
                    SelectGunByHotkey(idx);
                }
            });
        }
    }

    void SelectGun(int index) {
        // hide other guns
        for (int i = 0; i < gunsGameObjectParent.transform.childCount; i++) {
            gunsGameObjectParent.transform.GetChild(i).gameObject.SetActive(false);
        }
        selectedGunIndex = index;
        selectedGun = guns[selectedGunIndex];

        selectedGunObject = gunsGameObjectParent.transform.GetChild(selectedGunIndex).gameObject;
        selectedGunObject.SetActive(true);

        gunAnimator = selectedGunObject.GetComponent<Animator>();
        gunAudioSource = selectedGunObject.GetComponent<AudioSource>();

        HudManager.Instance.AdjustBulletsCount();
    }

    void SelectGunByHotkey(int hotkey) {
        SelectGun(hotkey - 1);
    }

    public void PlayerWalk(bool isWalking) {
        if (HudManager.Instance.IsPaused || isDead) {
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
        if (HudManager.Instance.IsPaused || isDead) {
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

        isBeingDamaged = true;
        playerController.CanMovePlayer = false;
        StartCoroutine(EnablePlayerMovementAndSetIsBeingDamagedAfterSeconds(1.5f));

        cameraAnimator.SetTrigger("Damage");

        HudManager.Instance.AdjustHealthBar(health, 100);
        HudManager.Instance.ShowBloodImage();

        if (health <= 0) {
            health = 0;
            isDead = true;
            HudManager.Instance.ShowGameOverImage();
        }
    }

    public void CollectAmmo(int bulletsAmount) {
        // add bullets to the current gun
        availableBullets[selectedGunIndex] += bulletsAmount;
        HudManager.Instance.AdjustBulletsCount();
    }

    private IEnumerator EnablePlayerMovementAndSetIsBeingDamagedAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isBeingDamaged = false;
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

    public Animator GunAnimator {
        get { return gunAnimator; }
    }

    public AudioSource GunAudioSource {
        get { return gunAudioSource; }
    }

    public int SelectedGunIndex {
        get { return selectedGunIndex; }
    }

    public GameObject SelectedGunObject {
        get { return selectedGunObject; }
    }

    public Weapon SelectedGun {
        get { return selectedGun; }
    }

    public int CurrentBullets {
        get { return currentBullets[selectedGunIndex]; }
        set {
            currentBullets[selectedGunIndex] = value;
        }
    }

    public int MaxBullets {
        get { return maxBullets[selectedGunIndex]; }
        set {
            maxBullets[selectedGunIndex] = value;
        }
    }

    public int AvailableBullets {
        get { return availableBullets[selectedGunIndex]; }
        set {
            availableBullets[selectedGunIndex] = value;
        }
    }
}
