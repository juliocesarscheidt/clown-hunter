using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    private GameObject player;
    private PlayerStats playerStats;

    public int paperTotalToCollect = 5;
    private int paperCollected = 0;
    public TextMeshProUGUI paperCounterText;

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

        AdjustPaperCounterText();
    }

    public void CollectPaper() {
        paperCollected++;
        AdjustPaperCounterText();

        if (paperCollected >= paperTotalToCollect) {
            HudManager.Instance.ShowEndGameImage();
        }
    }

    public void AdjustPaperCounterText() {
        paperCounterText.text = paperCollected + "/" + paperTotalToCollect;
    }
}
