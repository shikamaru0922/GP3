using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombCooldownUI : MonoBehaviour
{
    public PlayerController playerController; // PlayerController 的引用
    public Image cooldownFill; // 冷却条的填充部分

    private void Start()
    {
        // 动态查找 PlayerController
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController reference is missing and could not be found dynamically.");
                return;
            }
        }

        // 动态查找冷却条
        if (cooldownFill == null)
        {
            cooldownFill = transform.Find("CoolDownFill")?.GetComponent<Image>();
            if (cooldownFill == null)
            {
                Debug.LogError("CooldownFill reference is missing in BombCooldownUI.");
                return;
            }
        }
    }

    private void Update()
    {
        if (playerController == null || cooldownFill == null)
        {
            return; // 如果引用未设置，则停止执行
        }

        // 获取 PlayerController 中的冷却状态和计时器
        bool isCooldown = GetPrivateField<bool>(playerController, "isBombCooldown");
        float cooldownTimer = GetPrivateField<float>(playerController, "bombCooldownTimer");
        float cooldownDuration = playerController.bombCooldownDuration;

        if (isCooldown)
        {
            // 更新冷却条
            cooldownFill.fillAmount = 1 - (cooldownTimer / cooldownDuration);
        }
        else
        {
            // 冷却完成，填充满冷却条
            cooldownFill.fillAmount = 1;
        }
    }

    // 通过反射获取私有字段的值
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var fieldInfo = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field '{fieldName}' not found in {obj.GetType()}.");
            return default;
        }
        return (T)fieldInfo.GetValue(obj);
    }
}
