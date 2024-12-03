using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    public PlayerController playerController; // 引用玩家控制器

    // 两个独立的屏幕叠加 Image
    public Image damageOverlay; // 红色闪烁效果
    public Image speedDangerOverlay; // 速度危险效果

    public Color hurtColor = new Color(1, 0, 0, 0.5f); // 受伤时的红色
    public Color speedDangerColor = new Color(0, 0, 0, 0.5f); // 速度危险时的灰色
    public float flashDuration = 0.2f; // 红色闪烁持续时间

    private bool isFlashing = false; // 是否正在闪烁
    private int lastHealth; // 上一帧的血量

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController reference is missing in ScreenEffects.");
        }

        if (damageOverlay == null || speedDangerOverlay == null)
        {
            Debug.LogError("Screen overlay references are missing in ScreenEffects.");
        }

        // 初始化最后的血量为玩家的当前血量
        if (playerController != null)
        {
            lastHealth = playerController.currentHealth;
        }

        // 确保所有效果初始化为透明
        ResetOverlays();
    }

    private void Update()
    {
        if (playerController == null) return;

        // 检查速度危险
        CheckSpeedDanger();

        // 检查玩家血量变化
        CheckPlayerHealth();
    }

    private void CheckSpeedDanger()
    {
        if (playerController.rb.velocity.magnitude > playerController.landingSpeedThreshold)
        {
            // 显示速度危险效果
            SetSpeedDangerOverlay(speedDangerColor);
        }
        else
        {
            // 清除速度危险效果
            SetSpeedDangerOverlay(Color.clear);
        }
    }

    private void CheckPlayerHealth()
    {
        // 如果血量减少，则触发受伤效果
        if (playerController.currentHealth < lastHealth)
        {
            FlashHurtEffect();
        }

        // 更新最后的血量
        lastHealth = playerController.currentHealth;
    }

    public void FlashHurtEffect()
    {
        if (isFlashing) return; // 如果已经在闪烁，则跳过

        StartCoroutine(HurtFlashCoroutine());
    }

    private IEnumerator HurtFlashCoroutine()
    {
        isFlashing = true;

        // 显示红色闪烁效果
        SetDamageOverlay(hurtColor);

        // 等待闪烁持续时间
        yield return new WaitForSeconds(flashDuration);

        // 恢复透明
        SetDamageOverlay(Color.clear);

        isFlashing = false;
    }

    private void SetDamageOverlay(Color color)
    {
        if (damageOverlay != null)
        {
            damageOverlay.color = color;
        }
    }

    private void SetSpeedDangerOverlay(Color color)
    {
        if (speedDangerOverlay != null)
        {
            speedDangerOverlay.color = color;
        }
    }

    private void ResetOverlays()
    {
        SetDamageOverlay(Color.clear);
        SetSpeedDangerOverlay(Color.clear);
    }
}