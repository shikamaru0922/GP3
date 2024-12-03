using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackHole : MonoBehaviour
{
    public float pullSpeed = 10f;            // 吸引速度（测试时可以调整）
    public float maxSpeed = 30f;            // 最大速度
    public float acceleration = 2f;         // 加速度（控制速度增加的速度）
    public float maxScale = 10f;            // 黑洞的最大缩放大小
    public float growthRate = 1f;           // 黑洞增长的速度

    private GameObject player;
    private PlayerController playerController;
    private Rigidbody playerRb;
    private Transform playerTransform;
    private bool isPulling = false;

    private GameTimer gameTimer;            // 引用 GameTimer 脚本
    private float initialScale;             // 黑洞的初始缩放大小
    private float totalTime;                // 总游戏时间

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

        // 获取 GameTimer 脚本
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            totalTime = gameTimer.totalTime; // 获取总时间
        }
        else
        {
            Debug.LogError("GameTimer not found!");
        }

        // 记录黑洞的初始缩放大小
        initialScale = transform.localScale.x; // 假设初始缩放是均匀的
    }

    void Update()
    {
        // 更新黑洞的大小
        UpdateBlackHoleSize();

        // 检查是否需要拉拽玩家
        if (player != null && playerController != null && playerController.gravityNum == 0)
        {
            // 如果玩家的 gravityNum == 0，开始拉拽
            isPulling = true;
        }
        else
        {
            isPulling = false;
        }

        if (isPulling)
        {
            // 吸引玩家到黑洞位置，并增加速度
            PullPlayer();
        }
    }

    void UpdateBlackHoleSize()
    {
        if (gameTimer != null)
        {
            float elapsedTime = totalTime - gameTimer.remainingTime; // 已经过的时间
            float t = elapsedTime / totalTime; // 归一化时间，0 到 1

            // 计算新的缩放大小
            float newScale = Mathf.Lerp(initialScale, maxScale, t);
            transform.localScale = new Vector3(newScale, newScale, newScale);
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 当玩家进入触发器时，结束游戏
            GameOver();
        }
    }

    void GameOver()
    {
        // 实现游戏结束的逻辑
        // 例如，调用 GameManager 中的 GameOver() 方法

        // 暂停游戏时间
        Time.timeScale = 0f;

        // 显示游戏结束的 UI
        //GameManager gameManager = FindObjectOfType<GameManager>();
        /*if (gameManager != null)
        {
            gameManager.GameOver();
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }*/

        // 输出日志
        SceneManager.LoadScene("Opening");
        Debug.Log("Game Over! Player entered the black hole.");
    }
}
