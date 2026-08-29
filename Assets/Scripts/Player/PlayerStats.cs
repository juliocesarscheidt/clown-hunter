using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FirstPersonController))]
public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int health = 100;
    public bool isDead = false;

    public int criticalHitDamage = 100;
    public int defaultHitDamage = 0; // it comes from the selected gun
    [SerializeField]
    private int currentHitDamage = 0;
    private int addHitDamageAmount = 0;

    private FirstPersonController playerController;
    private Shooting playerShooting;

    public AudioSource stepsAudioSource;
    public AudioClip stepsWalkingAudioClip;
    public AudioClip stepsRunningAudioClip;
    public Animator cameraAnimator;

    public bool canShoot = false;
    public bool isReloading = false;
    public bool isAiming = false;

    public bool canReceiveDamage = true;
    public bool isBeingDamaged = false;
    private Coroutine setIsBeingDamagedCoroutine;
    public int damageVariation = 10;

    public GameObject[] shotParticleEffectPos;

    private Animator gunAnimator;
    private AudioSource gunAudioSource;

    private Dictionary<int, bool> gunsEnabled = new();
    public GameObject gunsGameObjectParent;
    public List<Weapon> guns;
    [SerializeField]
    private Weapon selectedGun;
    [SerializeField]
    private int selectedGunIndex = -1;
    public int defaultGunIndex = 0;
    [SerializeField]
    private GameObject selectedGunObject;
    [SerializeField]
    private GameObject currentGunReticle;

    public bool spendAmmo = true;
    [SerializeField]
    private List<int> currentBullets = new();
    [SerializeField]
    private List<int> maxBullets = new();
    [SerializeField]
    private List<int> availableBullets = new();

    public CinemachineVirtualCamera virtualCamera;
    public Camera gunsCamera;
    public int defaultFieldOfView = 50;

    public GameObject pointToMonsterAttack;

    private readonly Dictionary<string, int> animationHashes = new() {
        { "isReloading", Animator.StringToHash("isReloading") },
        { "isAiming", Animator.StringToHash("isAiming") },

    };

    void Awake() {
        playerController = GetComponent<FirstPersonController>();
        playerShooting = GetComponent<Shooting>();

        for (int i = 0; i < guns.Count; i++) {
            var gun = guns[i];

            currentBullets.Add(gun.currentBullets);
            maxBullets.Add(gun.maxBullets);
            availableBullets.Add(gun.availableBullets);
        }

        EnableDefaultGuns();
    }

    void Start() {}

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

        if (!HudManager.Instance.IsPaused && HudManager.Instance.IsRunningGame && !isDead) {
            if (!isBeingDamaged) {
                EnablePlayerMovementAndCamera();
            } else {
                playerController.CanMovePlayer = false;
                playerController.CanMoveCamera = true;
            }

            if (!isReloading) {
                // by numeric keys
                Enumerable.Range(1, guns.Count).ToList().ForEach(idx => {
                    if (Input.GetKeyDown(idx.ToString())) {
                        ChangeGunByHotkey(idx);
                    }
                });

                // by mouse scroll
                float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
                if (mouseScroll != 0) {
                    int nextGunIndex = selectedGunIndex + (mouseScroll > 0 ? 1 : -1);
                    while (true) {
                        if (nextGunIndex < 0) {
                            nextGunIndex = guns.Count - 1;
                        } else if (nextGunIndex >= guns.Count) {
                            nextGunIndex = 0;
                        }
                        // check if the nextGunIndex is from an enabled gun
                        if (gunsEnabled[nextGunIndex]) {
                            break;
                        }
                        nextGunIndex = nextGunIndex + (mouseScroll > 0 ? 1 : -1);
                    }
                    ChangeGun(nextGunIndex);
                }

                // TODO: by joystick
                /*
                float verticalJoystick = Input.GetAxis("JoystickVerticalButtons");
                if (verticalJoystick != 0) {
                    if (verticalJoystick == 1 && selectedGunIndex != 0) {
                        ChangeGunByHotkey(1);
                    } else if (verticalJoystick == -1 && selectedGunIndex != 1) {
                        ChangeGunByHotkey(2);
                    }
                }
                */
            }

        } else {
            DisablePlayerMovementAndCamera();
        }
    }

    void ChangeGun(int index) {
        if (!gunsEnabled[index]) {
            return;
        }
        if (selectedGunIndex == index) {
            return;
        }

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


        playerShooting.ResetShootTimeAndTimingToggleAim();
        ExitAimingState();

        if (currentGunReticle.TryGetComponent(out Image img)) {
            img.sprite = selectedGun.gunReticleImage;
        }

        defaultHitDamage = selectedGun.hitDamage;
        UpdateCurrentHitDamage();

        if (HudManager.Instance != null) {
            HudManager.Instance.AdjustBulletsCount();
        }
    }

    void ChangeGunByHotkey(int hotkey) {
        ChangeGun(hotkey - 1);
    }

    public void SetGunEnabled(int index, bool enabled) {
        gunsEnabled[index] = enabled;
    }

    public void EnableDefaultGuns() {
        for (int i = 0; i < guns.Count; i++) {
            var gun = guns[i];
            SetGunEnabled(i, gun.isEnabledByDefault);
        }
        if (selectedGunIndex != defaultGunIndex) {
            ChangeGun(defaultGunIndex);
        }
    }

    public void EnableAllGuns() {
        for (int i = 0; i < guns.Count; i++) {
            SetGunEnabled(i, true);
        }
    }

    public void ChangeAddHitDamageAmount(int addHitDamageAmount) {
        this.addHitDamageAmount = addHitDamageAmount;
        UpdateCurrentHitDamage();
    }

    private void UpdateCurrentHitDamage() {
        currentHitDamage = defaultHitDamage + addHitDamageAmount;
    }

    public int GetCurrentHitDamage() {
        return currentHitDamage;
    }

    public void EnterAimingState() {
        isAiming = true;

        if (GunAnimator != null) {
            GunAnimator.SetBool(animationHashes["isReloading"], false);
            GunAnimator.SetBool(animationHashes["isAiming"], true);
        }

        virtualCamera.m_Lens.FieldOfView = 35;
        gunsCamera.fieldOfView = 35;
    }

    public void ExitAimingState() {
        isAiming = false;
        canShoot = false;

        if (GunAnimator != null) {
            GunAnimator.SetBool(animationHashes["isAiming"], false);
        }

        virtualCamera.m_Lens.FieldOfView = defaultFieldOfView;
        gunsCamera.fieldOfView = defaultFieldOfView;
    }

    public void PlayerControllerWalk(bool isWalking) {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || isDead) {
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

    public void PlayerControllerRun(bool isRunning) {
        if (HudManager.Instance.IsPaused || !HudManager.Instance.IsRunningGame || isDead) {
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
        if (canReceiveDamage) {
            health = Mathf.Max(health - damage, 0);
        }

        if (!isDead) {
            if (!isBeingDamaged) cameraAnimator.SetTrigger("Damage");
            HudManager.Instance.AdjustHealthBar(health, maxHealth);
            HudManager.Instance.ShowBloodImage();
        }

        if (setIsBeingDamagedCoroutine != null) StopCoroutine(setIsBeingDamagedCoroutine);
        setIsBeingDamagedCoroutine = StartCoroutine(SetIsBeingDamagedFalsyAfterSeconds(1.5f));

        isBeingDamaged = true;

        if (health <= 0) {
            health = 0;
            isDead = true;
            HudManager.Instance.ShowGameOverImage();
        }
    }

    public void CollectGunSetEnabled(int index) {
        SetGunEnabled(index, true);
    }

    public void CollectAmmo(int bulletsAmount) {
        if (spendAmmo) {
            // add bullets to the current gun
            availableBullets[selectedGunIndex] += bulletsAmount;
            HudManager.Instance.AdjustBulletsCount();
        }
    }

    public void CollectFirstAid(int healthAmount) {
        if (canReceiveDamage) {
            health = Mathf.Min(health + healthAmount, maxHealth);
            HudManager.Instance.AdjustHealthBar(health, maxHealth);
        }
    }

    public void FillAllAmmo() {
        for (int i = 0; i < guns.Count; i++) {
            currentBullets[i] = maxBullets[i];
        }
        HudManager.Instance.AdjustBulletsCount();
    }

    public void FillHealth() {
        health = maxHealth;
        HudManager.Instance.AdjustHealthBar(health, maxHealth);
    }

    private IEnumerator SetIsBeingDamagedFalsyAfterSeconds(float seconds) {
        // wait
        yield return new WaitForSeconds(seconds);
        isBeingDamaged = false;
    }

    public void DisablePlayerMovementAndCamera() {
        playerController.CanMovePlayer = false;
        playerController.CanMoveCamera = false;
    }

    public void EnablePlayerMovementAndCamera() {
        playerController.CanMovePlayer = true;
        playerController.CanMoveCamera = true;
    }

    public bool ObjectIsInPointOfView(GameObject toCheck) {
        Bounds bounds = toCheck.GetComponentInChildren<Collider>().bounds;
        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(cameraFrustum, bounds)) {
            return true;
        }
        return false;
    }

    public void SetSpendStamina(bool spend) {
        playerController.SpendStamina = spend;
        if (!spend) playerController.CurrentStamina = playerController.MaxStamina;
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
            if (spendAmmo) currentBullets[selectedGunIndex] = value;
        }
    }

    public int MaxBullets {
        get { return maxBullets[selectedGunIndex]; }
        set {
            if (spendAmmo) maxBullets[selectedGunIndex] = value;
        }
    }

    public int AvailableBullets {
        get { return availableBullets[selectedGunIndex]; }
        set {
            if (spendAmmo) availableBullets[selectedGunIndex] = value;
        }
    }
}
