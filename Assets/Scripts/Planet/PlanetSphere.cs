using System.Collections;
using UnityEngine;

public class PlanetSphere : MonoBehaviour
{
    public float newGravityConstant = 0.2f; // New gravity constant for planet body
    private float currentGravityConstant;
    public float waitTime = 0.5f;
    public Planet planet;
    public PlayerController playerController;

    private Coroutine gravityChangeCoroutine;

    public GameObject interactableItemPrefab; // InteractableItem 的预制件

    private bool itemGenerated = false; // 用于跟踪是否已经生成过 InteractableItem

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            playerController.animator.SetTrigger("LandingTrigger");
            // 检查下落速度是否超过阈值
            float landingSpeed = Mathf.Abs(playerController.rb.velocity.magnitude);
            if (landingSpeed > playerController.landingSpeedThreshold)
            {
                // 扣除生命值
                playerController.TakeDamage(1);
            }
            playerController.standtargetAngel = gameObject.transform.position;
            // Start coroutine to change gravity after a delay
            if (gravityChangeCoroutine != null)
            {
                StopCoroutine(gravityChangeCoroutine); // Stop any existing coroutine to reset the timer
            }
            gravityChangeCoroutine = StartCoroutine(ChangeGravityAfterDelay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.standtargetAngel = Vector3.zero;
        }
    }

    private IEnumerator ChangeGravityAfterDelay()
    {
        yield return new WaitForSeconds(waitTime); // Wait for the specified time
        planet.currentGravityConstant = newGravityConstant; // Change to new gravity constant
        Debug.Log("Gravity constant changed after delay");

        // 放置 InteractableItem
        PlaceInteractableItem();
    }

    void PlaceInteractableItem()
    {
        // 检查是否已经生成过
        if (itemGenerated)
        {
            return; // 已经生成过，直接返回
        }

        // 标记为已生成
        itemGenerated = true;

        // 获取 GameObject 的半径
        float radius = GetObjectRadius();

        // 决定放置方向（从 GameObject 的中心到放置位置的方向）
        Vector3 placementDirection = transform.forward; // 您可以根据需要更改方向

        // 规范化方向向量
        placementDirection.Normalize();

        // 计算放置位置：中心点 + （方向向量 * 半径）
        Vector3 itemPosition = transform.position + placementDirection * radius;

        // 创建一个绕 x 轴旋转 90 度的旋转
        Quaternion itemRotation = Quaternion.Euler(90f, 0f, 0f);

        // 实例化 InteractableItem
        GameObject interactableItem = Instantiate(interactableItemPrefab, itemPosition, itemRotation);
    }

    float GetObjectRadius()
    {
        float radius = 0f;

        // 尝试获取 SphereCollider
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            // 考虑 GameObject 的缩放
            radius = sphereCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // 如果没有 SphereCollider，使用 Renderer.bounds
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // 使用最大边界尺寸的一半作为半径
                radius = renderer.bounds.extents.magnitude;
            }
            else
            {
                Debug.LogWarning("无法获取 GameObject 的尺寸，使用默认半径");
                radius = 1f; // 默认值
            }
        }

        return radius;
    }
}
