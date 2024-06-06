using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    private PlayerStats playerStats;

    public int paperTotalToCollect = 5;
    private int paperCollected = 0;
    public TextMeshProUGUI paperCounterText;

    public int enemiesToIncrease = 1;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();

        AdjustPaperCounterText();
    }

    public void CollectPaper() {
        paperCollected++;
        AdjustPaperCounterText();

        MonsterManager.Instance.enemiesToSpawn += enemiesToIncrease;
        MonsterManager.Instance.SpawnEnemies();

        if (paperCollected >= paperTotalToCollect) {
            HudManager.Instance.ShowEndGameImage();
        }
    }

    public void AdjustPaperCounterText() {
        paperCounterText.text = paperCollected + "/" + paperTotalToCollect;
    }
}
