using UnityEngine;

public class Planet : MonoBehaviour
{
    public float gravityRange = 50f;          // 引力范围的初始半径
    public Color gravityRangeColor = new Color(0f, 1f, 0f, 0.25f); // 引力范围的可视化颜色
    public GameObject gravityRangeVisual;     // 用于显示引力范围的球体预制体

    public float planetMass = 1000f;          // 星球的质量（根据游戏调整）
    public float gravityConstant = 0.1f;      // 引力常数 G（根据游戏调整）

    // 新增参数
    public float minScale = 0.5f;             // 星球的最小缩放比例
    public float maxScale = 2f;               // 星球的最大缩放比例
    public float scaleIntensity = 1f;         // 缩放强度系数
    public float gravityRangeMultiplier = 1f; // 引力范围与缩放比例的乘数

    private GameObject visualSphere;
    private Rigidbody playerRb;
    private Transform playerTransform;
    private Vector3 initialScale;
    private SphereCollider gravityCollider;

    void Start()
    {
        // 保存星球的初始缩放
        initialScale = transform.localScale;

        // 添加一个球形触发器作为引力范围
        gravityCollider = gameObject.AddComponent<SphereCollider>();
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
            // 获取玩家的 Rigidbody 和 Transform 组件
            playerRb = other.GetComponent<Rigidbody>();
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 停止对玩家施加引力
            playerRb = null;
            playerTransform = null;

            // 重置星球的缩放到初始值
            transform.localScale = initialScale;

            // 重置引力范围和可视化球体
            UpdateGravityRange(gravityRange);
        }
    }

    private void FixedUpdate()
    {
        if (playerRb != null && playerTransform != null)
        {
            // 计算方向和距离
            Vector3 direction = transform.position - playerTransform.position;
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

            // 根据距离调整星球的缩放
            AdjustPlanetScale(distance);
        }
    }

    void AdjustPlanetScale(float distance)
    {
        // 计算距离比例（0 到 1），越近比例越大
        float distanceRatio = (gravityRange - distance) / gravityRange;
        distanceRatio = Mathf.Clamp01(distanceRatio);

        // 根据距离比例和缩放强度计算新的缩放比例
        float scaleMultiplier = Mathf.Lerp(1f, maxScale, distanceRatio * scaleIntensity);
        scaleMultiplier = Mathf.Clamp(scaleMultiplier, minScale, maxScale);

        // 应用新的缩放
        transform.localScale = initialScale * scaleMultiplier;

        // 更新引力范围和可视化球体
        float newGravityRange = gravityRange * scaleMultiplier * gravityRangeMultiplier;
        UpdateGravityRange(newGravityRange);
    }

    void UpdateGravityRange(float newRange)
    {
        // 更新 SphereCollider 的半径
        gravityCollider.radius = newRange / transform.localScale.x; // 考虑缩放

        // 更新可视化的引力范围
        if (visualSphere != null)
        {
            visualSphere.transform.localScale = Vector3.one * newRange * 2f / transform.localScale.x;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中选中星球时，绘制引力范围
        Gizmos.color = gravityRangeColor;
        Gizmos.DrawWireSphere(transform.position, gravityRange);
    }
}
