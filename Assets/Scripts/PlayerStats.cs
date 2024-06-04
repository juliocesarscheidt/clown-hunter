using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public bool isDead = false;

    public int criticalHitDamage = 100;
    public int regularHitDamage = 50;

    public int[] currentBullets; // new int[] { 6, 30 }
    public int[] maxBullets; // new int[] { 6, 30 }
    public int[] availableBullets; // new int[] { 12, 60 }

    public void ApplyDamage(int damage) {
        health = Mathf.Max(health - damage, 0);

        HudManager.Instance.AdjustHealthBar(health, 100);
        HudManager.Instance.ShowBloodImage();

        if (health <= 0) {
            health = 0;
            isDead = true;
            HudManager.Instance.ShowGameOverImage();
        }
    }

    public void CollectAmmo() {
        for (int i = 0; i < availableBullets.Length; i++) {
            availableBullets[i] += maxBullets[i];
        }
    }
}
