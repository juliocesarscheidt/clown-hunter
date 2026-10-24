using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance { get; private set; }
    private PlayerStats playerStats;

    [Header("Stamina")]
    public Slider StaminaSlider;
    public Slider HealthSlider;
    public Color StaminaColorDefault;
    public Color StaminaColorLow;

    [Header("State Panels")]
    public GameObject GameOverImage;
    public GameObject EndGameImage;
    public GameObject PauseGamePanel;

    [Header("Options Panels")]
    public GameObject OptionsGamePanelBg;
    public List<GameObject> OptionsGamePanelLayers;

    [Header("Interact Overlay Canvas")]
    public GameObject InteractOverlayCanvas;
    [SerializeField]
    private bool isShowingOverlayCanvas;
    public GameObject OverlayCanvasSlot;
    private GameObject currentOverlayCanvasSlotItem;
    public TextMeshProUGUI overlayCanvasText;
    private Action onCloseAction;

    [Header("Damage UI")]
    public Image bloodImage;
    private bool showBloodImage = false;
    public float timeToShowBloodImage = 4f;
    private float showBloodImageTimer = 0f;
    public Color bloodImageColorDefault;
    
    [Header("Game States")]
    [SerializeField]
    private bool isPaused = false;
    [SerializeField]
    private bool isRunningGame = false;
    [SerializeField]
    private bool wasShowingInventory = false;
    [SerializeField]
    private bool wasShowingCutscene = false;

    [Header("UI Info")]
    public GameObject uiInfoWraperObject;
    public TextMeshProUGUI bulletsCounterText;
    public Image gunIconImage;

    [Header("UI Info Texts")]
    public TextMeshProUGUI tempOptionsInfoText;
    private bool showOptionsTempInfoText = false;
    private float showOptionsTempInfoTextTimer = 0f;

    public TextMeshProUGUI cheatActivatedText;
    private bool showCheatActivatedText = false;
    private float showCheatActivatedTextTimer = 0f;

    public TextMeshProUGUI generalInfoText;
    private bool showGeneralInfoText = false;
    private float showGeneralInfoTextTimer = 0f;
    private float timeToShowGeneralInfoText = 1.5f;

    [Header("Guns View Canvas")]
    public GameObject GunsViewCanvas;
    public List<Image> gunsViewImageTiles = new();
    public List<Image> gunsViewImageSlots = new();
    private bool showGunsViewCanvas = false;
    public float timeToShowGunsViewCanvas = 1.5f;
    private float showGunsViewCanvasTimer = 0f;

    [Header("FPS")]
    public bool showFps = false;
    public float updateFpsFrequency = 0.2f;
    private float updateFpsTimer;
    public TextMeshProUGUI fpsText;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Start() {
        isRunningGame = true;

        HUDHidePauseGamePanel();
        HUDHideInventoryPanel();

        PostProcessingManager.Instance.DisableBlur();
    }

    void Update() {
        if (!playerStats.isDead && isRunningGame) {
            if (Input.GetButtonDown("Return")) {
                if (isShowingOverlayCanvas) {
                    HUDHideOverlayCanvas();
                } else {
                    if (!isPaused) {
                        HUDShowPauseGamePanel();
                    } else {
                        HUDHidePauseGamePanel();
                    }
                }
            }

            if (!isPaused && !isShowingOverlayCanvas && !IsShowingCutscene) {
                if (Input.GetButtonDown("Inventory")) {
                    if (!IsShowingInventory) {
                        HUDShowInventoryPanel();
                    } else {
                        HUDHideInventoryPanel();
                    }
                }
            }

            // close panels
            if (Input.GetButton("Fire2")) {
                if (isPaused) {
                    HUDHidePauseGamePanel();
                } else  if (IsShowingInventory) {
                    HUDHideInventoryPanel();
                } else if (isShowingOverlayCanvas) {
                    HUDHideOverlayCanvas();
                } else if (IsShowingCutscene) {
                    // do nothing
                }
            }

            if (showBloodImage) {
                CheckBloodImage();
            }
        }

        if (showOptionsTempInfoText) {
            showOptionsTempInfoTextTimer += Time.unscaledDeltaTime;
            if (showOptionsTempInfoTextTimer >= 1.5f) {
                ActivateOptionsTempInfoText("");
                showOptionsTempInfoTextTimer = 0f;
            }
        }

        // timers
        if (showCheatActivatedText) {
            showCheatActivatedTextTimer += Time.unscaledDeltaTime;
            if (showCheatActivatedTextTimer >= 1.5f) {
                ActivateCheatCodeActivatedText("");
                showCheatActivatedTextTimer = 0f;
            }
        }

        if (showGeneralInfoText) {
            showGeneralInfoTextTimer += Time.deltaTime;
            if (showGeneralInfoTextTimer >= timeToShowGeneralInfoText) {
                ActivateGeneralInfoText("", 0f);
                showGeneralInfoTextTimer = 0f;
            }
        }

        if (showGunsViewCanvas) {
            showGunsViewCanvasTimer += Time.deltaTime;
            if (showGunsViewCanvasTimer >= timeToShowGunsViewCanvas) {
                HUDHideGunsViewCanvas();
            }
        }

        if (showFps) {
            fpsText.gameObject.SetActive(true);
            UpdateFpsDisplay();
        } else {
            fpsText.gameObject.SetActive(false);
        }
    }

    // adjust cursor when focus in and out
    // had to disable "Cursor Locked" option on StarterAssetsInputs script
    private void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus) {
            if (isRunningGame && !IsPaused) {
                HUDShowPauseGamePanel();
            }
        } else {
            if ((isRunningGame && IsPaused) || !isRunningGame) {
                UnlockCursor();
            }
        }
    }

    private void UpdateFpsDisplay() {
        float fps;
        updateFpsTimer -= Time.deltaTime;
        if (updateFpsTimer <= 0) {
            fps = 1f / Time.unscaledDeltaTime;
            fpsText.text = $"FPS: {Mathf.Round(fps)}";
            updateFpsTimer = updateFpsFrequency;
        }
    }

    private void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetFinishGameUi() {
        isRunningGame = false;
        isPaused = false;
        isShowingOverlayCanvas = false;

        PostProcessingManager.Instance.EnableBlur();

        ToggleUIInfoWrapper(false);
        UnlockCursor();
    }

    public void ShowGameOverImage() {
        SetFinishGameUi();
        GameOverImage.SetActive(true);
    }

    public void ShowEndGameImage() {
        SetFinishGameUi();
        EndGameImage.SetActive(true);
    }

    public void ToggleUIInfoWrapper(bool enabled) {
        uiInfoWraperObject.SetActive(enabled);
    }

    public void HUDCutsceneEnter() {
        ToggleUIInfoWrapper(false);
    }

    public void HUDCutsceneExit() {
        ToggleUIInfoWrapper(true);
    }

    public void HUDShowPauseGamePanel() {
        if (IsShowingInventory) {
            wasShowingInventory = true;
            HUDHideInventoryPanel();
        }
        if (IsShowingCutscene) {
            wasShowingCutscene = true;
        }

        isPaused = true;
        PostProcessingManager.Instance.EnableBlur();

        UnlockCursor();

        ToggleUIInfoWrapper(false);
        HideOptionsPanels();

        Time.timeScale = 0;
        PauseGamePanel.SetActive(true);
    }

    public void HUDHidePauseGamePanel() {
        isPaused = false;
        PostProcessingManager.Instance.DisableBlur();

        LockCursor();

        ToggleUIInfoWrapper(true);
        HideOptionsPanels();

        Time.timeScale = 1;
        PauseGamePanel.SetActive(false);

        // display inventory again if it was showing before pausing the game
        if (wasShowingInventory) {
            wasShowingInventory = false;
            HUDShowInventoryPanel();
        }
        if (wasShowingCutscene) {
            wasShowingCutscene = false;
            HUDCutsceneEnter();
        }
    }

    public void HUDShowInventoryPanel() {
        PostProcessingManager.Instance.EnableBlur();

        ToggleUIInfoWrapper(false);

        Time.timeScale = 0;
        InventoryManager.Instance.ShowInventoryPanel();
    }

    public void HUDHideInventoryPanel() {
        PostProcessingManager.Instance.DisableBlur();

        ToggleUIInfoWrapper(true);

        Time.timeScale = 1;
        InventoryManager.Instance.HideInventoryPanel();
    }

    public void InventoryEnterInspectMode() {
        UnlockCursor();
    }

    public void InventoryExitInspectMode() {
        LockCursor();
    }
    
    public void HUDShowOverlayCanvas() {
        isShowingOverlayCanvas = true;
        PostProcessingManager.Instance.EnableBlur();

        ToggleUIInfoWrapper(false);

        Time.timeScale = 0;

        overlayCanvasText.enabled = false;
        InteractOverlayCanvas.SetActive(true);
    }

    public void HUDHideOverlayCanvas() {
        isShowingOverlayCanvas = false;
        PostProcessingManager.Instance.DisableBlur();

        ToggleUIInfoWrapper(true);

        Time.timeScale = 1;

        overlayCanvasText.enabled = false;
        InteractOverlayCanvas.SetActive(false);

        if (onCloseAction != null) {
            onCloseAction.Invoke();
            onCloseAction = null;
        }
    }

    public void SpawnItemOnOverlayCanvasSlot(GameObject prefab, string text = "", Action onCloseAction = null) {
        var overriding = currentOverlayCanvasSlotItem != null && currentOverlayCanvasSlotItem.name != prefab.name;
        if (overriding) {
            Destroy(currentOverlayCanvasSlotItem);
            currentOverlayCanvasSlotItem = null;
        }
        if (onCloseAction != null) {
            this.onCloseAction = onCloseAction;
        }

        if (OverlayCanvasSlot != null && currentOverlayCanvasSlotItem == null) {
            currentOverlayCanvasSlotItem = Instantiate(prefab, OverlayCanvasSlot.transform);
            currentOverlayCanvasSlotItem.name = prefab.name;
            currentOverlayCanvasSlotItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            currentOverlayCanvasSlotItem.transform.localScale = Vector3.one;

            if (text != "") {
                overlayCanvasText.enabled = true;
                overlayCanvasText.text = text;
            } else {
                overlayCanvasText.enabled = false;
                overlayCanvasText.text = string.Empty;
            }
        }
    }

    public void HUDShowGunsViewCanvas(int highlightSlotIndex) {
        GunsViewCanvas.SetActive(true);
        showGunsViewCanvas = true;
        for (int i = 0; i < gunsViewImageTiles.Count; i++) {
            if (i == highlightSlotIndex) {
                gunsViewImageTiles[i].color = Color.red;
            } else {
                gunsViewImageTiles[i].color = Color.white;
            }
        }
        // reset the counter
        showGunsViewCanvasTimer = 0f;
    }

    public void HUDHideGunsViewCanvas() {
        GunsViewCanvas.SetActive(false);
        showGunsViewCanvas = false;
        // reset the counter
        showGunsViewCanvasTimer = 0f;
    }

    public void AddGunsViewImageToSlot(int slotIndex, Sprite gunSprite) {
        if (slotIndex < 0 || slotIndex >= gunsViewImageSlots.Count) {
            return;
        }
        gunsViewImageSlots[slotIndex].sprite = gunSprite;
        gunsViewImageSlots[slotIndex].enabled = true;
    }

    public void EnterOptionsPanel(PanelObject panel) {
        if (!isPaused) { return; }
        PauseGamePanel.SetActive(false);

        OptionsGamePanelBg.SetActive(true);
        for (int i = 0; i < panel.level; i++) {
            OptionsGamePanelLayers[i].SetActive(false);
        }
        OptionsGamePanelLayers[panel.level].SetActive(true);
    }

    public void ExitOptionsPanel(PanelObject panel) {
        if (!isPaused) { return; }
        OptionsGamePanelLayers[panel.level].SetActive(false);

        // return to parent panel or close and return to pause panel
        if (panel.level > 0 && panel.parent != null) {
            EnterOptionsPanel(panel.parent);
        } else if (panel.level == 0) {
            OptionsGamePanelBg.SetActive(false);
            PauseGamePanel.SetActive(true);
        }
    }

    private void HideOptionsPanels() {
        for (int i = 0; i < OptionsGamePanelLayers.Count; i++) {
            OptionsGamePanelLayers[i].SetActive(false);
        }
        OptionsGamePanelBg.SetActive(false);
    }

    public void ApplyOptions() {
        SettingsManager.Instance.ApplySoundSettings();
        SettingsManager.Instance.ApplyDifficultySettings();
        SettingsManager.Instance.ApplyResolutionSettings();
        SettingsManager.Instance.ApplyHoldToggleAimSettings();
        ActivateOptionsTempInfoText("Options saved");
    }

    public void ActivateOptionsTempInfoText(string text) {
        showOptionsTempInfoText = text.Length > 0;
        tempOptionsInfoText.enabled = showOptionsTempInfoText;
        tempOptionsInfoText.text = text;
    }

    public void ActivateCheatCodeActivatedText(string text) {
        showCheatActivatedText = text.Length > 0;
        cheatActivatedText.gameObject.SetActive(showCheatActivatedText);
        cheatActivatedText.text = text;
    }

    public void ActivateGeneralInfoText(string text, float timeToShow = 1.5f) {
        timeToShowGeneralInfoText = timeToShow;

        showGeneralInfoText = text.Length > 0;
        generalInfoText.gameObject.SetActive(showGeneralInfoText);
        generalInfoText.text = text;
    }

    public void RestartGame() {
        isRunningGame = false;
        isPaused = false;
        isShowingOverlayCanvas = false;
        
        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(1));
    }

    public void GoToMenu() {
        UnlockCursor();
        ToggleUIInfoWrapper(false);

        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(0));
    }

    public void Quit() {
        UnlockCursor();
        ToggleUIInfoWrapper(false);

        Time.timeScale = 1;
        LevelLoaderManager.Instance.Quit();
    }

    public void AdjustBulletsCount() {
        if (playerStats != null && playerStats.SelectedGun != null) {
            bulletsCounterText.text = $"{playerStats.CurrentBullets} [{playerStats.AvailableBullets}]";
        }
    }

    public void AdjustGunImageSprite(Sprite gunSprite) {
        gunIconImage.sprite = gunSprite;
    }

    public void AdjustHealthBar(int currentHealth, int maxHealth) {
        HealthSlider.maxValue = maxHealth;
        HealthSlider.value = currentHealth;
    }

    public void AdjustStaminaBar(float currentStamina, float maxStamina, bool lowOnStamina) {
        StaminaSlider.gameObject.SetActive(currentStamina < maxStamina);

        StaminaSlider.value = currentStamina / maxStamina;

        Image sliderImage = StaminaSlider.fillRect.GetComponent<Image>();
        Color targetColor = lowOnStamina ? StaminaColorLow : StaminaColorDefault;
        sliderImage.color = Color.Lerp(sliderImage.color, targetColor, Time.deltaTime * 5f);
    }

    public void ShowBloodImage() {
        showBloodImageTimer = 0f;
        showBloodImage = true;
        bloodImage.enabled = true;
        bloodImage.color = bloodImageColorDefault;
    }

    private void HideBloodImage() {
        showBloodImageTimer = 0f;
        showBloodImage = false;
        bloodImage.enabled = false;
    }

    public void CheckBloodImage() {
        showBloodImageTimer += Time.deltaTime;
        bloodImage.color -= new Color(0f, 0f, 0f, 1f) * (Time.deltaTime / timeToShowBloodImage);

        if (showBloodImageTimer >= timeToShowBloodImage) {
            HideBloodImage();
        }
    }

    public bool IsRunningGame {
        get { return isRunningGame; }
    }

    public bool IsPaused {
        get { return isPaused; }
    }

    public bool IsShowingOverlayCanvas {
        get { return isShowingOverlayCanvas; }
    }

    public bool IsShowingInventory {
        get { return InventoryManager.Instance.IsShowingInventory; }
    }

    public bool IsShowingCutscene {
        get { return CutsceneManager.Instance.IsShowingCutscene; }
    }
}
