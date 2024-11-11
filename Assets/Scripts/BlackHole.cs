using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public float pullSpeed = 10f;            // 吸引速度（测试时可以调整）
    public float maxSpeed = 30f;            // 最大速度
    public float acceleration = 2f;         // 加速度（控制速度增加的速度）
    
    private GameObject player;
    private PlayerController playerController;
    private Rigidbody playerRb;
    private Transform playerTransform;
    private bool isPulling = false;

    void Start()
    {
        // 查找玩家并获取 PlayerController 和 Rigidbody
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerRb = player.GetComponent<Rigidbody>();
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }

    void Update()
    {
        if (player != null && playerController != null && playerController.gravityNum == 0)
        {
            // 如果玩家的 gravityNum == 0，开始拉拽
            isPulling = true;
        }
        if (player != null && playerController != null && playerController.gravityNum != 0)
        {
            // 如果玩家的 gravityNum == 0，开始拉拽
            isPulling = false;
        }
        
        if (isPulling)
        {
            // 吸引玩家到黑洞位置，并增加速度
            PullPlayer();
        }
    }

    void PullPlayer()
    {
        if (playerRb != null && playerTransform != null)
        {
            Debug.Log("Pulling");
            // 计算拉拽方向
            Vector3 direction = transform.position - playerTransform.position;
            float distance = direction.magnitude;

            // 计算速度，增加加速度直到达到最大速度
            float currentSpeed = Mathf.Min(pullSpeed + acceleration * Time.deltaTime, maxSpeed);
            
            // 施加力来拉拽玩家
            Vector3 force = direction.normalized * currentSpeed;
            playerRb.AddForce(force);

            // 如果玩家距离黑洞很近，停止拉拽
            if (distance < 1f) // 距离小于1时停止拉拽
            {
                isPulling = false;
                playerRb.velocity = Vector3.zero; // 停止玩家的运动
            }
        }
    }
}