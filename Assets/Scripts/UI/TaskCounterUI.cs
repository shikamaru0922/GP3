using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskCounterUI : MonoBehaviour
{
    public PlayerController playerController; // ���� PlayerController
    public Text taskCounterText; // ��ʾ����Ŀ���������ı�

    private void Start()
    {
        // ȷ�����ò�Ϊ��
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
        // ��������Ŀ�������� UI
        if (playerController != null && taskCounterText != null)
        {
            taskCounterText.text = $"{playerController.taskTargetCount}/3";
        }
    }
}