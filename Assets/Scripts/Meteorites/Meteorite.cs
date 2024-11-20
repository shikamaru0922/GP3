using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float speed = 10f; // 陨石移动速度
    public float lifeTime = 20f; // 陨石的生命周期
    public GameObject explosionEffect; // 爆炸特效预制件

    private Transform playerTransform;
    public bool playerAttached = false; // 是否有玩家附着在陨石上
    public GameObject playerObject = null; // 玩家对象的引用

    public float rotationSpeed = 5f;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 朝向玩家的方向
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.forward = direction;
        if (playerAttached)
            Debug.Log("111");
            //

        // 销毁陨石
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 如果玩家不存在，返回
        if (playerTransform == null)
            return;

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
            Debug.Log("Player attached");
            AttachPlayer(other.gameObject);
        }
    }

    void AttachPlayer(GameObject player)
    {
        playerObject = player; 
        player.transform.SetParent(transform);
        playerAttached = true;
        rotationSpeed = 0f;//metrorite 停止旋转
        // 禁用玩家控制
        /*PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }*/
    }

    void DetachPlayer()
    {
        if (playerObject != null)
        {
            playerObject.transform.SetParent(null);
            playerAttached = false;
            
            // 清空玩家对象的引用
            playerObject = null;

            // 可选：播放脱离动画或效果
            Debug.Log("Player detached from meteorite.");
        }
    }

    public void DestroyMeteorite()
    {
        // 实例化爆炸特效
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 如果玩家被吸附在陨石上，解除吸附
        if (playerAttached)
        {
            DetachPlayer();
        }

        // 销毁陨石
        Destroy(gameObject);
    }
}
