using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerController playerController; // 引用玩家控制器
    public Image healthBarFill; // 血条的填充部分

    private void Start()
    {
        // 检查引用是否正确
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
            // 计算当前血量百分比
            float healthPercentage = (float)playerController.currentHealth / playerController.maxHealth;
            // 更新血条填充比例
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercentage);
        }
    }
}