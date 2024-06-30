using System.Collections.Generic;
using TMPro;
using UnityEngine;

class Task {
    public string description;
    public int currentProgress;
    public int totalProgress;
    public bool isInProgress;
    public bool isCompleted;

    public Task (string description, int totalProgress) {
        this.description = description;
        currentProgress = 0;
        this.totalProgress = totalProgress;
        isInProgress = false;
        isCompleted = false;
    }

    public void StartTask() {
        isInProgress = true;
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

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    public TextMeshProUGUI taskInfoText;

    public enum TaskType : int {
        FindAndCollectNewspapers = 0,
        EliminateTheRemainingClowns = 1,
    }

    [SerializeField]
    private int currentTaskIndex = 0;

    private readonly Dictionary<int, Task> tasks = new() {
        { (int)TaskType.FindAndCollectNewspapers, new Task("Find and collect newspapers", 0) }, // the totalProgress is dynamic
        { (int)TaskType.EliminateTheRemainingClowns, new Task("Eliminate the remaining clowns", 0) }, // the totalProgress is dynamic
    };
 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    public void StartTask(int taskIndex) {
        tasks[taskIndex].StartTask();
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
        return $"- {tasks[currentTaskIndex].description} ({tasks[currentTaskIndex].currentProgress}/{tasks[currentTaskIndex].totalProgress})";
    }

    private void UpdateTaskInfoText(string text) {
        taskInfoText.text = text;
    }
}
