using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject newGamePanel;
    public GameObject optionsGamePanelBg;
    public List<GameObject> optionsGamePanelLayers;
    public TextMeshProUGUI tempInfoText;
    private bool showTempInfoText = false;
    private float showTempInfoTextTimer = 0f;

    void Update() {
        if (showTempInfoText) {
            showTempInfoTextTimer += Time.unscaledDeltaTime;
            if (showTempInfoTextTimer >= 1.5f) {
                SetAndActivateTempInfoText("");
                showTempInfoTextTimer = 0f;
            }
        }
    }

    public void LoadNextLevel() {
        LevelLoaderManager.Instance.LoadNextLevel();
    }

    public void Quit() {
        LevelLoaderManager.Instance.Quit();
    }

    public void EnterOptionsPanel(PanelObject panel) {
        newGamePanel.SetActive(false);

        optionsGamePanelBg.SetActive(true);
        for (int i = 0; i < panel.level; i++) {
            optionsGamePanelLayers[i].SetActive(false);
        }
        optionsGamePanelLayers[panel.level].SetActive(true);
    }

    public void ExitOptionsPanel(PanelObject panel) {
        optionsGamePanelLayers[panel.level].SetActive(false);

        // return to parent panel or close and return to pause panel
        if (panel.level > 0 && panel.parent != null) {
            EnterOptionsPanel(panel.parent);
        } else if (panel.level == 0) {
            optionsGamePanelBg.SetActive(false);
            newGamePanel.SetActive(true);
        }
    }

    public void ApplyOptions() {
        SettingsManager.Instance.ApplySoundSettings();
        SettingsManager.Instance.ApplyDifficultySettings();
        SettingsManager.Instance.ApplyResolutionSettings();
        SetAndActivateTempInfoText("Options saved");
    }

    private void SetAndActivateTempInfoText(string text) {
        showTempInfoText = text.Length > 0;
        tempInfoText.gameObject.SetActive(showTempInfoText);
        tempInfoText.text = text;
    }
}
