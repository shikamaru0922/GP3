using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float speed = 10f;            // 陨石移动速度
    public float lifeTime = 20f;         // 陨石的生命周期
    public GameObject explosionEffect;   // 爆炸特效预制件

    private Transform playerTransform;
    public bool playerAttached = false;  // 是否有玩家附着在陨石上
    public GameObject playerObject = null; // 玩家对象的引用

    public float rotationSpeed = 5f;

    void Start()
    {
        // 获取玩家的 Transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        // 朝向玩家的方向
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.forward = direction;
        }

        // 如果玩家已经附着，输出调试信息
        if (playerAttached)
        {
            Debug.Log("Player is already attached to the meteorite.");
        }

        // 在 lifeTime 秒后销毁陨石
        Invoke(nameof(DestroyMeteorite), lifeTime);
    }

    void Update()
    {
        // 如果玩家不存在，尝试重新获取
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // 如果仍然找不到玩家，停止后续操作
                return;
            }
        }

        // 计算方向向量
        Vector3 direction = playerTransform.position - transform.position;

        // 计算目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 设置物体的旋转，使其朝向玩家
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 向前移动
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerAttached)
        {
            Debug.Log("Player attached to meteorite.");
            AttachPlayer(other.gameObject);
        }
        if (other.CompareTag("Gravity"))
        {
            Debug.Log("Meteorite collided with grabity.");
            DestroyMeteorite();
        }
    }

    // 如果您使用的是碰撞而非触发器，请使用以下方法
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Meteorite collided with Ground.");
            DestroyMeteorite();
        }
    }
    */

    void AttachPlayer(GameObject player)
    {
        playerObject = player;
        player.transform.SetParent(transform, true); // 确保玩家的世界位置保持不变
        playerAttached = true;
        rotationSpeed = 0f; // 停止陨石旋转

        // 可选：禁用玩家控制
        /*
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        */
    }

    void DetachPlayer()
    {
        if (playerObject != null)
        {
            playerObject.transform.SetParent(null, true); // 确保玩家的世界位置保持不变
            playerAttached = false;

            // 可选：重新启用玩家控制
            /*
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            */

            // 清空玩家对象的引用
            playerObject = null;

            // 可选：播放脱离动画或效果
            Debug.Log("Player detached from meteorite.");
        }
    }

    public void DestroyMeteorite()
    {
        // 首先检查玩家是否附着在陨石上
        if (playerAttached)
        {
            // 如果玩家附着，先分离玩家
            DetachPlayer();
        }

        // 实例化爆炸特效
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 销毁陨石
        Destroy(gameObject);
    }
}
