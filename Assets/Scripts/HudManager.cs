using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public GameObject pressEGameObject;
    public GameObject optionsPanel;

    public Image bloodImage;
    private bool showBloodImage = false;
    public float timeToShowBloodImage = 2f;
    private float showBloodImageTimer = 0f;
    public Color bloodImageColorDefault;

    private GameObject player;
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
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();

        LockCursor();
        reticle.SetActive(true);

        AdjustBulletsCount();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !playerStats.isDead) {
            if (!isPaused) {
                ShowOptions();
            } else {
                HideOptions();
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
        pressEGameObject.SetActive(true);
    }

    public void HidePressEObject() {
        pressEGameObject.SetActive(false);
    }

    public void ShowOptions() {
        isPaused = true;
        playerStats.DisablePlayerMovementAndCamera();

        UnlockCursor();
        reticle.SetActive(false);

        Time.timeScale = 0;
        optionsPanel.SetActive(true);
    }

    public void HideOptions() {
        isPaused = false;
        playerStats.EnablePlayerMovementAndCamera();

        LockCursor();
        reticle.SetActive(true);

        Time.timeScale = 1;
        optionsPanel.SetActive(false);
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
        if (player.TryGetComponent<Shooting>(out var playerShooting)) {
            bulletsCounterText.text =
                $"{playerStats.currentBullets[playerShooting.SelectedGun()]}/{playerStats.availableBullets[playerShooting.SelectedGun()]}";
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
        bloodImage.color -= new Color(0f, 0f, 0f, 1f) * (Time.deltaTime / 2);

        if (showBloodImageTimer >= timeToShowBloodImage) {
            showBloodImage = false;
            bloodImage.enabled = false;
            showBloodImageTimer = 0;
        }
    }
}
