using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance { get; private set; }

    public Slider StaminaSlider;
    public Slider HealthSlider;

    public Color StaminaColorDefault;
    public Color StaminaColorLow;

    public GameObject GameOverImage;
    public GameObject EndGameImage;
    public GameObject PauseGamePanel;
    public TextMeshProUGUI PressInteractText;

    public GameObject OptionsGamePanelBg;
    public List<GameObject> OptionsGamePanelLayers;

    public Image bloodImage;
    private bool showBloodImage = false;
    public float timeToShowBloodImage = 4f;
    private float showBloodImageTimer = 0f;
    public Color bloodImageColorDefault;

    private PlayerStats playerStats;
    [SerializeField]
    private bool isPaused = false;
    [SerializeField]
    private bool isRunningGame = false;

    public GameObject uiInfoWraperObject;
    public TextMeshProUGUI bulletsCounterText;
    public Image gunIconImage;

    public TextMeshProUGUI tempInfoText;
    private bool showTempInfoText = false;
    private float showTempInfoTextTimer = 0f;

    public TextMeshProUGUI cheatActivatedText;
    private bool showCheatActivatedText = false;
    private float showCheatActivatedTextTimer = 0f;

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
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        isRunningGame = true;
        HidePauseGamePanel();
        AdjustBulletsCount();
    }

    void Update() {
        if (!playerStats.isDead && isRunningGame) {
            if (Input.GetButtonDown("Return")) {
                if (!isPaused) {
                    ShowPauseGamePanel();
                } else {
                    HidePauseGamePanel();
                }
            }
            if (showBloodImage) {
                CheckBloodImage();
            }
        }

        if (showTempInfoText) {
            showTempInfoTextTimer += Time.unscaledDeltaTime;
            if (showTempInfoTextTimer >= 1.5f) {
                SetAndActivateTempInfoText("");
                showTempInfoTextTimer = 0f;
            }
        }

        if (showCheatActivatedText) {
            showCheatActivatedTextTimer += Time.unscaledDeltaTime;
            if (showCheatActivatedTextTimer >= 1.5f) {
                SetAndActivateCheatActivatedText("");
                showCheatActivatedTextTimer = 0f;
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
                ShowPauseGamePanel();
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

    private void FinishGameUi() {
        isRunningGame = false;

        HidePressInteractObject();
        uiInfoWraperObject.SetActive(false);

        UnlockCursor();
    }

    public void ShowGameOverImage() {
        FinishGameUi();
        GameOverImage.SetActive(true);
    }

    public void ShowEndGameImage() {
        FinishGameUi();
        EndGameImage.SetActive(true);
    }

    public void ShowPressInteractObject() {
        PressInteractText.gameObject.SetActive(true);
    }

    public void HidePressInteractObject() {
        PressInteractText.gameObject.SetActive(false);
    }

    public void SetPressInteractTextToMouseKeyboard() {
        PressInteractText.text = "Press [E] to interact";
    }

    public void SetPressInteractTextToXBoxJoystick() {
        PressInteractText.text = "Press (Y) to interact";
    }

    public void ShowPauseGamePanel() {
        isPaused = true;

        UnlockCursor();
        uiInfoWraperObject.SetActive(false);

        Time.timeScale = 0;

        HideOptionsPanels();
        PauseGamePanel.SetActive(true);
    }

    public void HidePauseGamePanel() {
        isPaused = false;

        LockCursor();
        uiInfoWraperObject.SetActive(true);

        Time.timeScale = 1;

        HideOptionsPanels();
        PauseGamePanel.SetActive(false);
    }

    private void HideOptionsPanels() {
        for (int i = 0; i < OptionsGamePanelLayers.Count; i++) {
            OptionsGamePanelLayers[i].SetActive(false);
        }
        OptionsGamePanelBg.SetActive(false);
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

    public void ApplyOptions() {
        SettingsManager.Instance.ApplySoundSettings();
        SettingsManager.Instance.ApplyDifficultySettings();
        SettingsManager.Instance.ApplyResolutionSettings();
        SetAndActivateTempInfoText("Options saved");
    }

    public void SetAndActivateTempInfoText(string text) {
        showTempInfoText = text.Length > 0;
        tempInfoText.enabled = showTempInfoText;
        tempInfoText.text = text;
    }

    public void SetAndActivateCheatActivatedText(string text) {
        showCheatActivatedText = text.Length > 0;
        cheatActivatedText.gameObject.SetActive(showCheatActivatedText);
        cheatActivatedText.text = text;
    }

    public void RestartGame() {
        isRunningGame = false;
        isPaused = false;
        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(1));
    }

    public void GoToMenu() {
        UnlockCursor();
        uiInfoWraperObject.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(0));
    }

    public void Quit() {
        UnlockCursor();
        uiInfoWraperObject.SetActive(false);
        Time.timeScale = 1;
        LevelLoaderManager.Instance.Quit();
    }

    public void AdjustBulletsCount() {
        if (playerStats != null) {
            bulletsCounterText.text =
                $"{playerStats.CurrentBullets}/{playerStats.AvailableBullets}";
            gunIconImage.sprite = playerStats.SelectedGun.gunIconImage;
        }
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
}
