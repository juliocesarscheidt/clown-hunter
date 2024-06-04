using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderManager : MonoBehaviour
{
    public static LevelLoaderManager Instance { get; private set; }

    private Animator animator;
    public float transitionTime = 1f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        animator = GetComponentInChildren<Animator>();
    }

    public void LoadNextLevel() {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public IEnumerator LoadLevel(int levelIndex) {
        // play animation
        animator.SetTrigger("Start");
        // wait
        yield return new WaitForSeconds(transitionTime);
        // load scene
        SceneManager.LoadScene(levelIndex);
    }

    public void LoadMenu() {
        StartCoroutine(LoadLevel(0));
    }

    public void Quit() {
        Application.Quit();
    }
}
