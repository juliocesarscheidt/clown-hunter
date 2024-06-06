using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance { get; private set; }

    public Slider StaminaSlider;
    public Slider HealthSlider;

    public Color StaminaColorDefault;
    public Color StaminaColorLow;

    public GameObject GameOverImage;
    public GameObject EndGameImage;
    public GameObject PressEGameObject;
    public GameObject PauseGamePanel;

    public GameObject OptionsGamePanelBg;
    public List<GameObject> OptionsGamePanelLayers;

    public Image bloodImage;
    private bool showBloodImage = false;
    public float timeToShowBloodImage = 4f;
    private float showBloodImageTimer = 0f;
    public Color bloodImageColorDefault;

    private PlayerStats playerStats;
    private bool isPaused = false;

    public GameObject reticle;

    public TextMeshProUGUI bulletsCounterText;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        LockCursor();
        reticle.SetActive(true);

        AdjustBulletsCount();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !playerStats.isDead) {
            if (!isPaused) {
                ShowPauseGamePanel();
            } else {
                HidePauseGamePanel();
            }
        }

        if (showBloodImage && !playerStats.isDead) {
            CheckBloodImage();
        }
    }

    private void LockCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowGameOverImage() {
        GameOverImage.SetActive(true);
        playerStats.DisablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);
    }

    public void ShowEndGameImage() {
        EndGameImage.SetActive(true);
        playerStats.DisablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);
    }

    public void ShowPressEObject() {
        PressEGameObject.SetActive(true);
    }

    public void HidePressEObject() {
        PressEGameObject.SetActive(false);
    }

    public void ShowPauseGamePanel() {
        isPaused = true;
        playerStats.DisablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);

        Time.timeScale = 0;

        HideOptionsPanels();
        PauseGamePanel.SetActive(true);
    }

    public void HidePauseGamePanel() {
        isPaused = false;
        playerStats.EnablePlayerMovementAndCamera();

        LockCursor();
        reticle.SetActive(true);

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
        VolumeManager.Instance.ApplySoundSettings();
    }

    public void RestartGame() {
        isPaused = false;
        playerStats.EnablePlayerMovementAndCamera();

        LockCursor();
        reticle.SetActive(true);

        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(1));
    }

    public void GoToMenu() {
        isPaused = false;
        playerStats.EnablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);

        Time.timeScale = 1;
        StartCoroutine(LevelLoaderManager.Instance.LoadLevel(0));
    }

    public void Quit() {
        isPaused = false;
        playerStats.EnablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);

        Time.timeScale = 1;
        LevelLoaderManager.Instance.Quit();
    }

    public bool IsPaused() {
        return isPaused;
    }

    public void AdjustBulletsCount() {
        if (playerStats != null) {
            bulletsCounterText.text =
                $"{playerStats.CurrentBullets}/{playerStats.AvailableBullets}";
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
        showBloodImageTimer = 0;

        showBloodImage = true;

        bloodImage.enabled = true;
        bloodImage.color = bloodImageColorDefault;
    }

    public void CheckBloodImage() {
        showBloodImageTimer += Time.deltaTime;
        bloodImage.color -= new Color(0f, 0f, 0f, 1f) * (Time.deltaTime / timeToShowBloodImage);

        if (showBloodImageTimer >= timeToShowBloodImage) {
            showBloodImage = false;
            bloodImage.enabled = false;
            showBloodImageTimer = 0;
        }
    }
}
