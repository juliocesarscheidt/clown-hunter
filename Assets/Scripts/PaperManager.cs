using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    public int paperTotalToCollect = 5;
    private int paperCollected = 0;
    public TextMeshProUGUI paperCounterText;

    public int enemiesToIncreaseOnPaperCollected = 2;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        AdjustPaperCounterText();
    }

    public void CollectPaper() {
        paperCollected++;
        AdjustPaperCounterText();

        MonsterManager.Instance.enemiesToSpawn += enemiesToIncreaseOnPaperCollected;
        MonsterManager.Instance.SpawnEnemiesDelayed();

        if (paperCollected >= paperTotalToCollect) {
            HudManager.Instance.ShowEndGameImage();
        }
    }

    public void AdjustPaperCounterText() {
        paperCounterText.text = paperCollected + "/" + paperTotalToCollect;
    }
}
