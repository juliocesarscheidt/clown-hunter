using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    public GameObject cutsceneCameraObject;
    private Animator cutsceneCameraAnimator;

    [SerializeField]
    private bool isShowingCutscene = false;
    public float timeToPlayCutscene = 3.0f;
    private float playCutsceneTimer = 0f;

    public enum CutsceneType {
        ShowSelectedDoorCutscene,
    }

    private readonly Dictionary<CutsceneType, int> animationHashes = new() {
        { CutsceneType.ShowSelectedDoorCutscene, Animator.StringToHash("ShowSelectedDoorCutscene") },
    };

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        cutsceneCameraAnimator = cutsceneCameraObject.GetComponent<Animator>();
    }

    void LateUpdate() {
        if (!GlobalGameplayManager.Instance.IsGameplayActiveForCutscene) {
            return;
        }

        if (isShowingCutscene) {
            playCutsceneTimer += Time.deltaTime;
            if (playCutsceneTimer >= timeToPlayCutscene) {
                isShowingCutscene = false;
                playCutsceneTimer = 0f;
                cutsceneCameraObject.SetActive(false);

                HudManager.Instance.HUDCutsceneExit();
            }
        }
    }
  
    public void PlayCutscene(CutsceneType cutscene, float timeToPlayCutscene) {
        this.timeToPlayCutscene = timeToPlayCutscene;
        isShowingCutscene = true;
        HudManager.Instance.HUDCutsceneEnter();

        if (cutscene == CutsceneType.ShowSelectedDoorCutscene) {
            var cameraPos = DoorsManager.Instance.GetSortedDoorCameraPosition();
            if (cameraPos != Vector3.zero) {
                cutsceneCameraObject.transform.position = cameraPos;
            }
            var cameraRot = DoorsManager.Instance.GetSortedDoorCameraRotation();
            if (cameraRot != Quaternion.identity) {
                cutsceneCameraObject.transform.rotation = cameraRot;
            }
        }

        cutsceneCameraObject.SetActive(true);

        if (animationHashes.TryGetValue(cutscene, out int animationHash)) {
            cutsceneCameraAnimator.Play(animationHash);
        }
    }

    public bool IsShowingCutscene {
        get { return isShowingCutscene; }
    }
}
