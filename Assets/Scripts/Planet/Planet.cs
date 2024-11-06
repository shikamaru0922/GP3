using UnityEngine;

public class Planet : MonoBehaviour
{
    public float gravityRange = 50f;          // 引力范围的半径
    public Color gravityRangeColor = new Color(0f, 1f, 0f, 0.25f); // 引力范围的可视化颜色
    public GameObject gravityRangeVisual;     // 用于显示引力范围的球体预制体

    public float planetMass = 1000f;          // 星球的质量（根据游戏调整）
    public float gravityConstant = 0.1f;      // 引力常数 G（根据游戏调整）

    private GameObject visualSphere;
    private Rigidbody playerRb;

    void Start()
    {
        // 添加一个球形触发器作为引力范围
        SphereCollider gravityCollider = gameObject.AddComponent<SphereCollider>();
        gravityCollider.isTrigger = true;
        gravityCollider.radius = gravityRange / transform.localScale.x; // 考虑到缩放因素

        // 创建用于可视化引力范围的球体
        if (gravityRangeVisual != null)
        {
            visualSphere = Instantiate(gravityRangeVisual, transform.position, Quaternion.identity, transform);
            visualSphere.transform.localScale = Vector3.one * gravityRange * 2f / transform.localScale.x;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 获取玩家的 Rigidbody 组件
            playerRb = other.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 停止对玩家施加引力
            playerRb = null;
        }
    }

    private void FixedUpdate()
    {
        if (playerRb != null)
        {
            // 计算方向和距离
            Vector3 direction = transform.position - playerRb.position;
            float distance = direction.magnitude;

            // 防止距离过小导致引力过大
            float minDistance = 1f;
            distance = Mathf.Max(distance, minDistance);

            // 计算引力大小 F = G * m1 * m2 / r^2
            float playerMass = playerRb.mass;
            float forceMagnitude = gravityConstant * planetMass * playerMass / (distance * distance);

            // 计算引力方向
            Vector3 force = direction.normalized * forceMagnitude;

            // 将力施加到玩家身上
            playerRb.AddForce(force);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中选中星球时，绘制引力范围
        Gizmos.color = gravityRangeColor;
        Gizmos.DrawWireSphere(transform.position, gravityRange);
    }
}
