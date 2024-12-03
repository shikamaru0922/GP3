using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerController playerController; // ������ҿ�����
    public Image healthBarFill; // Ѫ������䲿��

    private void Start()
    {
        // ��������Ƿ���ȷ
        if (playerController == null)
        {
            Debug.LogError("PlayerController reference is missing in HealthBar.");
        }

        if (healthBarFill == null)
        {
            Debug.LogError("HealthBarFill reference is missing in HealthBar.");
        }
    }

    private void Update()
    {
        if (playerController != null && healthBarFill != null)
        {
            // ���㵱ǰѪ���ٷֱ�
            float healthPercentage = (float)playerController.currentHealth / playerController.maxHealth;
            // ����Ѫ��������
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercentage);
        }
    }
}