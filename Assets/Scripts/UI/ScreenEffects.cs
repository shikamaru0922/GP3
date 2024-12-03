using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    public PlayerController playerController; // ������ҿ�����

    // ������������Ļ���� Image
    public Image damageOverlay; // ��ɫ��˸Ч��
    public Image speedDangerOverlay; // �ٶ�Σ��Ч��

    public Color hurtColor = new Color(1, 0, 0, 0.5f); // ����ʱ�ĺ�ɫ
    public Color speedDangerColor = new Color(0, 0, 0, 0.5f); // �ٶ�Σ��ʱ�Ļ�ɫ
    public float flashDuration = 0.2f; // ��ɫ��˸����ʱ��

    private bool isFlashing = false; // �Ƿ�������˸
    private int lastHealth; // ��һ֡��Ѫ��

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

        // ��ʼ������Ѫ��Ϊ��ҵĵ�ǰѪ��
        if (playerController != null)
        {
            lastHealth = playerController.currentHealth;
        }

        // ȷ������Ч����ʼ��Ϊ͸��
        ResetOverlays();
    }

    private void Update()
    {
        if (playerController == null) return;

        // ����ٶ�Σ��
        CheckSpeedDanger();

        // ������Ѫ���仯
        CheckPlayerHealth();
    }

    private void CheckSpeedDanger()
    {
        if (playerController.rb.velocity.magnitude > playerController.landingSpeedThreshold)
        {
            // ��ʾ�ٶ�Σ��Ч��
            SetSpeedDangerOverlay(speedDangerColor);
        }
        else
        {
            // ����ٶ�Σ��Ч��
            SetSpeedDangerOverlay(Color.clear);
        }
    }

    private void CheckPlayerHealth()
    {
        // ���Ѫ�����٣��򴥷�����Ч��
        if (playerController.currentHealth < lastHealth)
        {
            FlashHurtEffect();
        }

        // ��������Ѫ��
        lastHealth = playerController.currentHealth;
    }

    public void FlashHurtEffect()
    {
        if (isFlashing) return; // ����Ѿ�����˸��������

        StartCoroutine(HurtFlashCoroutine());
    }

    private IEnumerator HurtFlashCoroutine()
    {
        isFlashing = true;

        // ��ʾ��ɫ��˸Ч��
        SetDamageOverlay(hurtColor);

        // �ȴ���˸����ʱ��
        yield return new WaitForSeconds(flashDuration);

        // �ָ�͸��
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