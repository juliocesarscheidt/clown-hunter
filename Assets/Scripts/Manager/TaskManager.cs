using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

class Task {
    public string description;
    public int currentProgress;
    public int totalProgress;
    public bool isInProgress;
    public bool isCompleted;
    [System.NonSerialized]
    public Action<int> onStartAction;

    public Task (string description, int totalProgress, Action<int> onStartAction = null) {
        this.description = description;
        currentProgress = 0;
        this.totalProgress = totalProgress;
        isInProgress = false;
        isCompleted = false;
        this.onStartAction = onStartAction;
    }

    public void StartTask(int globalTaskIndex) {
        isInProgress = true;
        onStartAction?.Invoke(globalTaskIndex);
    }

    public void UpdateProgress(int progress) {
        currentProgress += progress;
        if (currentProgress >= totalProgress) {
            currentProgress = totalProgress;
            isInProgress = false;
            isCompleted = true;
        }
    }
}

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance { get; private set; }
    public TextMeshProUGUI taskInfoText;

    public AudioClip firstTaskTalkAudioClip;
    public AudioClip thirdTaskTalkAudioClip;

    private PlayerStats playerStats;

    public enum TaskType : int {
        InvestigateThePlace = 0,
        EliminateTheRemainingEnemies = 1,
        EscapeFromThePlace = 2,
    }

    [SerializeField]
    private int currentTaskIndex = 0;

    private Dictionary<int, Task> tasks = new();
 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerStats = FindObjectOfType<PlayerStats>();

        tasks = new() {
            { (int) TaskType.InvestigateThePlace, new Task("Investigate the place\n  collect evidences", 0, (int index) => {
                Debug.Log($"started task {index}");
                if (playerStats != null) {
                   playerStats.ReproduceTalk(firstTaskTalkAudioClip);
                }
                if (HudManager.Instance != null) {
                    HudManager.Instance.ActivateGeneralInfoText("I'm trapped here with these monsters, I need to find a way out.", 5.0f);
                }
            })},
            { (int) TaskType.EliminateTheRemainingEnemies, new Task("Eliminate the remaining enemies", 0, (int index) => {
                Debug.Log($"started task {index}");
                // stop spawning enemies
                MonsterManager.Instance.SetCanSpawnEnemies(false);
                // spawn the special weapon
                InventoryManager.Instance.ActivateSpecialWeapon();
            })},
            { (int) TaskType.EscapeFromThePlace, new Task("Escape from the place", 0, (int index) => {
                Debug.Log($"started task {index}");
                if (playerStats != null) {
                   playerStats.ReproduceTalk(thirdTaskTalkAudioClip);
                }
                if (HudManager.Instance != null) {
                    HudManager.Instance.ActivateGeneralInfoText("Ok, now I need to find a key.", 5.0f);
                }

                TreasureChestManager.Instance.ToggleTreasureChest(true);
                DoorsManager.Instance.SetupDoors();
            })},
        };
    }

    private void Start() {
        StartTask((int) TaskType.InvestigateThePlace);
    }

    public void StartTask(int taskIndex) {
        tasks[taskIndex].StartTask(taskIndex);
    }

    public bool IsCurrentTask(int taskIndex) {
        return taskIndex == currentTaskIndex;
    }

    public void UpdateTaskTotalProgress(int taskIndex, int totalProgress) {
        if (!tasks[taskIndex].isInProgress && !tasks[taskIndex].isCompleted) {
            tasks[taskIndex].totalProgress = totalProgress;
        }
        UpdateTaskInfoText(GenerateTaskInfoText());
    }

    public void UpdateTaskProgress(int taskIndex, int addedProgress) {
        // only update the current task
        if (taskIndex != currentTaskIndex) {
            return;
        }
        if (!tasks[taskIndex].isInProgress) {
            StartTask(taskIndex);
        }

        tasks[currentTaskIndex].UpdateProgress(addedProgress);
        UpdateTaskInfoText(GenerateTaskInfoText());

        if (tasks[currentTaskIndex].isCompleted) {
            currentTaskIndex++;

            // if there are more tasks, go to the next one
            if (currentTaskIndex < tasks.Count) {
                StartTask(currentTaskIndex);
                UpdateTaskInfoText(GenerateTaskInfoText());

            // otherwise, end the game
            } else {
                HudManager.Instance.ShowEndGameImage();
            }
        }
    }

    private string GenerateTaskInfoText() {
        if (tasks[currentTaskIndex].totalProgress > 1) {
            return $"- {tasks[currentTaskIndex].description} [{tasks[currentTaskIndex].currentProgress}/{tasks[currentTaskIndex].totalProgress}]";
        } else {
            return $"- {tasks[currentTaskIndex].description}";
        }
    }

    private void UpdateTaskInfoText(string text) {
        taskInfoText.text = text;
    }
}
