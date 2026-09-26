using Cinemachine;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public AudioSource gunsAudioSource;

    private Dictionary<int, bool> gunsEnabled = new();
    public List<Weapon> guns;
    public List<GameObject> gunsGameObjects;
    [SerializeField]
    private Weapon selectedGun;
    [SerializeField]
    private int selectedGunIndex = -1;
    public int defaultGunIndex = 0;
    [SerializeField]
    private GameObject selectedGunObject;
    private float changeGunTimer = 0f;
    public float changeGunInterval = 0.2f;
    private bool isChangingGun = false;

    public GameObject currentGunReticle;
    private bool isReticleRed = false;
    private float reticleRedTimer = 0f;
    public float reticleRedInterval = 0.5f;

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

    public GameObject flashlightInventoryItemPrefab;
    public GameObject nightvisionBinocularInventoryItemPrefab;

    private readonly Dictionary<string, int> animationHashes = new() {
        { "isReloading", Animator.StringToHash("isReloading") },
        { "isAiming", Animator.StringToHash("isAiming") },
    };

    void Awake() {
        playerController = GetComponent<FirstPersonController>();
        playerShooting = GetComponent<Shooting>();

        void equipAction(InventoryItem item) {
            HudManager.Instance.HideInventoryPanel();
            ChangeGun(item.defaultItemIndex);
        }
        for (int i = 0; i < guns.Count; i++) {
            var gun = guns[i];

            // dynamically set inventory item in the gun
            gun.SetInventoryItemData(i, gun.gunName, true, true, equipAction, null);

            currentBullets.Add(gun.currentBullets);
            maxBullets.Add(gun.maxBullets);
            availableBullets.Add(gun.availableBullets);
        }
    }

    private void Start() {
        EnableDefaultGuns();
        if (HudManager.Instance != null) {
            HudManager.Instance.AdjustBulletsCount();
        }
        AddDefaultInventoryItems();
    }

    void Update() {
        if (HudManager.Instance.IsRunningGame) {
            if (GlobalGameplayManager.Instance.IsGameplayPaused) {
                gunsAudioSource.Pause();
                stepsAudioSource.Pause();
            } else {
                gunsAudioSource.UnPause();
                stepsAudioSource.UnPause();
            }
        }

        if (GlobalGameplayManager.Instance.IsGameplayActive) {
            if (!isBeingDamaged) {
                EnablePlayerMovementAndCamera();
            } else {
                playerController.CanMovePlayer = false;
                playerController.CanMoveCamera = true;
            }

            if (isReticleRed) {
                ChangeReticleToColorWithTimer(Color.red);
                reticleRedTimer += Time.deltaTime;
                if (reticleRedTimer > reticleRedInterval) {
                    isReticleRed = false;
                    reticleRedTimer = 0f;
                }
            } else {
                ChangeReticleToColorWithTimer(Color.white);
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
                if (mouseScroll != 0 && !isChangingGun) {
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
                        nextGunIndex += (mouseScroll > 0 ? 1 : -1);
                    }
                    isChangingGun = true;
                    ChangeGun(nextGunIndex);
                }

                float verticalJoystick = Input.GetAxis("JoystickVerticalButtons");
                if (verticalJoystick != 0 && !isChangingGun) {
                    int nextGunIndex = selectedGunIndex + (verticalJoystick > 0 ? 1 : -1);
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
                        nextGunIndex += (verticalJoystick > 0 ? 1 : -1);
                    }
                    isChangingGun = true;
                    ChangeGun(nextGunIndex);
                }

                if (isChangingGun) {
                    changeGunTimer += Time.deltaTime;
                    if (changeGunTimer > changeGunInterval) {
                        isChangingGun = false;
                        changeGunTimer = 0f;
                    }
                }
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
        for (int i = 0; i < gunsGameObjects.Count; i++) {
            gunsGameObjects[i].SetActive(false);
        }
        selectedGunIndex = index;
        selectedGun = guns[selectedGunIndex];

        selectedGunObject = gunsGameObjects[selectedGunIndex];
        selectedGunObject.SetActive(true);

        gunAnimator = selectedGunObject.GetComponent<Animator>();

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

    public void ChangeReticleToColorWithTimer(Color color) {
        if (currentGunReticle.TryGetComponent(out Image img)) {
            img.color = color;
        }
    }

    public void SetIsReticleRed(bool isReticleRed) {
        this.isReticleRed = isReticleRed;
        reticleRedTimer = 0f;
    }

    private void AddDefaultInventoryItems() {
        if (flashlightInventoryItemPrefab != null) {
            void onEquipActionFlashlight(InventoryItem item) {
                HudManager.Instance.HideInventoryPanel();
                FlashlightManager.Instance.TurnOn();
            }
            InventoryItem flashlightInventory = new(0, "Flashlight", InventoryItem.InteractableType.Item,
                true, true, onEquipActionFlashlight, null);
            InventoryManager.Instance.AddInteractableItem(flashlightInventory, flashlightInventoryItemPrefab);
        }

        if (nightvisionBinocularInventoryItemPrefab != null) {
            void onEquipActionBinocular(InventoryItem item) {
                HudManager.Instance.HideInventoryPanel();
                NightVisionManager.Instance.TurnOn();
            }
            InventoryItem binocularInventory = new(1, "Night Vision Binocular", InventoryItem.InteractableType.Item,
                true, true, onEquipActionBinocular, null);
            InventoryManager.Instance.AddInteractableItem(binocularInventory, nightvisionBinocularInventoryItemPrefab);
        }
    }

    public void SetGunEnabled(int index, bool enabled) {
        var alreadyEnabled = gunsEnabled.Count > index && gunsEnabled[index];
        gunsEnabled[index] = enabled;

        if (!alreadyEnabled && enabled) {
            if (InventoryManager.Instance != null && guns.Count > index) {
                var gun = guns[index];
                InventoryManager.Instance.AddInteractableItem(gun.inventoryItem, gun.baseInventoryItemPrefab);
            }
        }
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
        if (!GlobalGameplayManager.Instance.IsGameplayActive) {
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
        if (!GlobalGameplayManager.Instance.IsGameplayActive) {
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

        // set to black and white, and after a few seconds it will be set to the previous profile inside SetIsBeingDamagedFalsyAfterSeconds
        PostProcessingManager.Instance.SetBlackWhiteProfile();

        if (setIsBeingDamagedCoroutine != null) StopCoroutine(setIsBeingDamagedCoroutine);
        setIsBeingDamagedCoroutine = StartCoroutine(SetIsBeingDamagedFalsyAfterSeconds(1.5f));

        isBeingDamaged = true;

        if (health <= 0) {
            Die();
        }
    }

    public void Die() {
        health = 0;
        isDead = true;
        HudManager.Instance.ShowGameOverImage();
    }

    public void CollectGunSetEnabled(int index) {
        SetGunEnabled(index, true);
        ChangeGun(index);
    }

    public void CollectAmmunition(int bulletsAmount) {
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

    public void FillAllAmmunition() {
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
        // return to the previous profile
        PostProcessingManager.Instance.SetPreviousProfile();
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
        // Gets the Axis-Aligned Bounding Box (AABB) of that collider in world space.
        // An AABB is a 3D box (with width, height, and depth) that completely encloses the object's collider, aligned with the world axes
        Bounds bounds = toCheck.GetComponentInChildren<Collider>().bounds;
        // Generates an array of 6 3D planes representing the camera's viewing pyramid (the frustum): Left, Right, Bottom, Top, Near, and Far clipping planes
        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        // Tests whether the target object's bounding box (bounds) overlaps or resides inside the 6 camera planes (cameraFrustum).
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
