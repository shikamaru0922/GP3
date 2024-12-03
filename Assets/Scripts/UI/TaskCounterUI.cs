using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskCounterUI : MonoBehaviour
{
    public PlayerController playerController; // 引用 PlayerController
    public Text taskCounterText; // 显示任务目标数量的文本

    private void Start()
    {
        // 确保引用不为空
        if (playerController == null)
        {
            Debug.LogError("PlayerController reference is missing in TaskCounterUI.");
        }

        if (taskCounterText == null)
        {
            Debug.LogError("TaskCounterText reference is missing in TaskCounterUI.");
        }
    }

    private void Update()
    {
        // 更新任务目标数量的 UI
        if (playerController != null && taskCounterText != null)
        {
            taskCounterText.text = $"{playerController.taskTargetCount}/3";
        }
    }
}