using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetSphere : MonoBehaviour
{
    public float newGravityConstant = 0.2f; // 新的重力常数
    private float currentGravityConstant;
    public float waitTime = 0.5f; // 等待时间
    public Planet planet;
    public PlayerController playerController;

    private Coroutine gravityChangeCoroutine;

    public GameObject interactableItemPrefab; // InteractableItem 的预制件

    public GameObject interactableItemInstance = null; // 生成的 InteractableItem 的引用
    private Coroutine deactivateItemCoroutine = null; // 使 InteractableItem 不激活的协程引用
    
    public bool hasGeneratedInteractableItem = false; // Flag to track item generation

    private void Update()
    {
        // 如果标志为 true 且未能生成实例（interactableItemInstance为null），则修改当前物体和父物体的Layer为Default
        if (hasGeneratedInteractableItem && interactableItemInstance == null)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (transform.parent != null)
            {
                transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            playerController.animator.SetTrigger("LandingTrigger");
            playerController.standtargetAngel = gameObject.transform.position;
            // Existing gravity change logic
            if (gravityChangeCoroutine != null)
            {
                StopCoroutine(gravityChangeCoroutine);
            }
            gravityChangeCoroutine = StartCoroutine(ChangeGravityAfterDelay());

            // Reactivate or place the item as needed
            if (interactableItemInstance != null && !interactableItemInstance.activeSelf)
            {
                interactableItemInstance.SetActive(true);
                Debug.Log("InteractableItem reactivated");
            }
            else if (!hasGeneratedInteractableItem)
            {
                PlaceInteractableItem();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.standtargetAngel = Vector3.zero;

            // Deactivate the item after a delay if it exists and is active
            if (interactableItemInstance != null && interactableItemInstance.activeSelf)
            {
                if (deactivateItemCoroutine != null)
                {
                    StopCoroutine(deactivateItemCoroutine);
                }
                deactivateItemCoroutine = StartCoroutine(DeactivateItemAfterDelay(0.5f));
            }
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
        // If the item has already been generated, do not generate it again
        if (hasGeneratedInteractableItem)
        {
            if (interactableItemInstance != null && !interactableItemInstance.activeSelf)
            {
                interactableItemInstance.SetActive(true);
                Debug.Log("InteractableItem reactivated in PlaceInteractableItem");
            }
            return;
        }

        // Get the radius and calculate the placement position and rotation
        float radius = GetObjectRadius();
        Vector3 placementDirection = transform.forward; // Adjust as needed
        placementDirection.Normalize();
        Vector3 itemPosition = transform.position + placementDirection * radius;
        Quaternion itemRotation = Quaternion.identity; // 可以根据需要微调旋转

        // Instantiate the InteractableItem
        if (interactableItemPrefab != null)
        {
            interactableItemInstance = Instantiate(interactableItemPrefab, itemPosition, itemRotation);
        }

        // 设置标志位为 true，表示已经生成
        hasGeneratedInteractableItem = true;

       
    }

    private IEnumerator DeactivateItemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only deactivate if the item exists and is active
        if (interactableItemInstance != null && interactableItemInstance.activeSelf)
        {
            interactableItemInstance.SetActive(false);
            Debug.Log("InteractableItem deactivated after delay");
        }
        deactivateItemCoroutine = null;
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
                radius = 1f; // 默认值
            }
        }
        Debug.Log("Radius: " + radius);
        return radius;
    }
}
