using System.Collections;
using UnityEngine;

public class PlanetSphere : MonoBehaviour
{
    public float newGravityConstant = 0.2f; // 新的重力常数
    private float currentGravityConstant;
    public float waitTime = 0.5f; // 等待时间
    public Planet planet;
    public PlayerController playerController;

    private Coroutine gravityChangeCoroutine;

    public GameObject interactableItemPrefab; // InteractableItem 的预制件

    private GameObject interactableItemInstance = null; // 生成的 InteractableItem 的引用
    private Coroutine deactivateItemCoroutine = null; // 使 InteractableItem 不激活的协程引用

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            playerController.animator.SetTrigger("LandingTrigger");
            //playerController.audioSource.PlayOneShot(landingSound);
            // 检查下落速度是否超过阈值
            float landingSpeed = Mathf.Abs(playerController.rb.velocity.magnitude);
            if (landingSpeed > playerController.landingSpeedThreshold)
            {
                // 扣除生命值
                playerController.TakeDamage(1);
            }
            playerController.standtargetAngel = gameObject.transform.position;
            // 启动协程，延迟改变重力
            if (gravityChangeCoroutine != null)
            {
                StopCoroutine(gravityChangeCoroutine); // 停止已有的协程
            }
            gravityChangeCoroutine = StartCoroutine(ChangeGravityAfterDelay());

            // 如果有正在运行的协程，停止它
            if (deactivateItemCoroutine != null)
            {
                StopCoroutine(deactivateItemCoroutine);
                deactivateItemCoroutine = null;
            }

            // 如果 InteractableItem 存在且未激活，重新激活它
            if (interactableItemInstance != null && !interactableItemInstance.activeSelf)
            {
                interactableItemInstance.SetActive(true);
                Debug.Log("InteractableItem reactivated");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.standtargetAngel = Vector3.zero;
            // 启动协程，1 秒后将 InteractableItem 设置为不激活
            if (deactivateItemCoroutine != null)
            {
                StopCoroutine(deactivateItemCoroutine);
            }
            deactivateItemCoroutine = StartCoroutine(DeactivateItemAfterDelay(1f)); // 1 秒延迟
        }
    }

    private IEnumerator ChangeGravityAfterDelay()
    {
        yield return new WaitForSeconds(waitTime); // 等待指定时间
        planet.currentGravityConstant = newGravityConstant; // 更改重力常数
        Debug.Log("Gravity constant changed after delay");

        // 放置 InteractableItem
        PlaceInteractableItem();
    }

    void PlaceInteractableItem()
    {
        // 如果 InteractableItem 已经存在且激活，直接返回
        if (interactableItemInstance != null && interactableItemInstance.activeSelf)
        {
            return;
        }

        // 如果 InteractableItem 已经存在但未激活，重新激活它
        if (interactableItemInstance != null && !interactableItemInstance.activeSelf)
        {
            interactableItemInstance.SetActive(true);
            Debug.Log("InteractableItem reactivated in PlaceInteractableItem");
            return;
        }

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
        interactableItemInstance = Instantiate(interactableItemPrefab, itemPosition, itemRotation);
    }

    private IEnumerator DeactivateItemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 将 InteractableItem 设置为不激活
        if (interactableItemInstance != null && interactableItemInstance.activeSelf)
        {
            interactableItemInstance.SetActive(false);
            Debug.Log("InteractableItem deactivated after delay");
        }
        deactivateItemCoroutine = null; // 清除协程引用
    }

    float GetObjectRadius()
    {
        float radius = 0f;

        // 尝试获取 SphereCollider
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Debug.Log("SphereCollider");
            // 考虑 GameObject 的缩放
            radius = sphereCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            Debug.Log("No SphereCollider");
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
        Debug.Log("Radius: " + radius);
        return radius;
    }
}
