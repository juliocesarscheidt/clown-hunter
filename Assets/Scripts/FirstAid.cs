using UnityEngine;

public class FirstAid : MonoBehaviour, Interactable
{
    private PlayerStats playerStats;
    public int healthAmount = 100;

    void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    public void Collect() {
        HudManager.Instance.HidePressEObject();
        playerStats.CollectFirstAid(healthAmount);
    }
}
