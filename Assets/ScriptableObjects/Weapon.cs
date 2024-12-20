using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject {
    public string gunName;
    public GameObject gunPrefabObject;
    public GameObject particleShotEffect;
    public GameObject gunPrefabBullet;

    public AudioClip gunShotSound;
    public AudioClip gunReloadSound;
    public AudioClip gunEmptySound;

    public Sprite gunIconImage;

    public float timeToShootInterval;
    public float timeToReloadInterval = 3f;

    public bool isAutomaticGun;

    public int currentBullets;
    public int maxBullets;
    public int availableBullets;
}
