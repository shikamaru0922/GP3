using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject bombPrefab;          // 炸弹预制件
    public Transform bombSpawnPoint;       // 炸弹生成位置
    public float throwForce = 15f;         // 投掷力度
    public float moveSpeed = 5f;           // 移动速度
    public float maxspeed_inSpace = 30;
    public float mouseSensitivity = 2f;    // 鼠标灵敏度

    private Rigidbody rb;
    private bool isGrounded;
    private Bomb currentBomb;
    private float xRotation = 0f;

    private Vector3 startPosition;         // 初始位置

    public int gravityNum;
    public int playerScore;
    public float probeLength = 1.0f; // 探针的长度，可在Inspector中调节
    public Color probeColor = Color.green; // 探针的颜色
    
    private Vector3 startPoint;        // 起点（通常为当前 GameObject 的位置）
    private Vector3 endPoint;          // 初始方向的终点
    public Vector3 standtargetAngel;  // 目标方向的终点

    [Header("Rotation Settings")]
    public float rotationSpeed = 1f;  // 旋转速度（度/秒）
    private bool isRotating = false;  // 标记旋转是否正在进行
    
    [Header("Bomb Settings")]
    public int maxBombsBeforeCooldown = 2;    // 扔几次炸弹后进入冷却
    public float bombCooldownDuration = 5f;   // 冷却持续时间，可在 Inspector 中调整

    private int bombsThrown = 0;              // 已投掷的炸弹次数
    private float bombCooldownTimer = 0f;     // 冷却计时器
    private bool isBombCooldown = false;      // 是否处于冷却状态

      
    // New variables for raycasting interaction
    [Header("Interaction Settings")]
    public float interactionRange = 5f;                    // Maximum interaction distance
    private List<GameObject> interactedItems = new List<GameObject>(); // List of interacted items
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        // 保存初始位置
        startPosition = transform.position;
        
        //调整角度
        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + Vector3.down * probeLength;
    }

    void Update()
    {
        LookAround();
        HandleBombThrow();
        HandleBombDetonate();
        HandleResetPosition();
        HandleInteraction(); 
        // 处理炸弹冷却计时器
        if (isBombCooldown)
        {
            bombCooldownTimer += Time.deltaTime;
            if (bombCooldownTimer >= bombCooldownDuration)
            {
                // 冷却结束，重置计数和状态
                bombsThrown = 0;
                isBombCooldown = false;
                bombCooldownTimer = 0f;
                Debug.Log("炸弹冷却结束，可以再次投掷炸弹。");
            }
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        AnimateAngle();
        MaxSpeedCheck();
    }

    public void AnimateAngle()
    {
        // 设置探针的起始点为当前对象的位置
        Vector3 startPoint = transform.position;

        // 探针的方向是物体的下方
        Vector3 endPoint = startPoint + transform.TransformDirection(Vector3.down) * probeLength;

        // 如果 standtargetAngel 不为零，开始旋转
        if (standtargetAngel != Vector3.zero)
        {
            if (!isRotating)
            {
                isRotating = true;

                // 计算目标方向（从 startPoint 指向 standtargetAngel）
                Vector3 targetDirection = standtargetAngel - startPoint;

                // 计算需要的旋转，使 transform.down 对齐到目标方向
                Quaternion targetRotation = Quaternion.FromToRotation(transform.TransformDirection(Vector3.down), targetDirection.normalized) * transform.rotation;

                // 计算当前旋转和目标旋转之间的角度差
                float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

                // 计算基于旋转速度的持续时间
                float duration = angleDifference / rotationSpeed;

                // 开始旋转动画
                transform.DORotateQuaternion(targetRotation, duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => isRotating = false);
            }
        }
        else
        {
            if (isRotating)
            {
                // 停止旋转
                transform.DOKill();
                isRotating = false;
            }
            // 可选：在此处重置旋转或保持当前旋转
        }}

    
    private void OnDrawGizmos()
    {
        // 设置探针的起始点为当前对象的位置
        Vector3 startPoint = transform.position;
        
        // 探针的方向是物体的下方
        Vector3 endPoint = startPoint + Vector3.down * probeLength;

        // 设置 Gizmos 的颜色，方便可视化
        Gizmos.color = probeColor;
        
        // 绘制探针的线段
        Gizmos.DrawLine(startPoint, endPoint);
        Gizmos.DrawLine(startPoint, standtargetAngel);

        // 在探针的末端绘制一个小球作为终点标记
        Gizmos.DrawSphere(endPoint, 0.05f); // 小球的半径可以调节
    }


    public void MaxSpeedCheck()
    {
        if (gravityNum == 0)
        {
            if (rb.velocity.magnitude > maxspeed_inSpace)
            rb.velocity = rb.velocity.normalized * maxspeed_inSpace;
            Debug.Log(rb.velocity.magnitude);
        }
        
        
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void MovePlayer()
    {
        float h = Input.GetAxis("Horizontal"); // A/D 键
        float v = Input.GetAxis("Vertical");   // W/S 键

        // 获取摄像机的前方向和右方向
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 根据输入计算移动方向
        Vector3 moveDirection = cameraForward * v + cameraRight * h;

        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        
        
            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + move);
  
        
    }

    void HandleBombThrow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isBombCooldown && bombsThrown < maxBombsBeforeCooldown)
            {
                GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);
                Rigidbody bombRb = bomb.GetComponent<Rigidbody>();

                bombRb.velocity = rb.velocity;

                Vector3 throwDirection = Camera.main.transform.forward;
                bombRb.AddForce(throwDirection.normalized * throwForce, ForceMode.VelocityChange);

                currentBomb = bomb.GetComponent<Bomb>();

                bombsThrown++;

                if (bombsThrown >= maxBombsBeforeCooldown)
                {
                    isBombCooldown = true;
                    bombCooldownTimer = 0f;
                }
            }
            else
            {
                // 可选：提供反馈，告知玩家处于冷却状态
                Debug.Log("无法投掷炸弹：处于冷却状态。");
            }
        }
    }


    void HandleBombDetonate()
    {
        if (Input.GetMouseButtonDown(1) && currentBomb != null)
        {
            currentBomb.Detonate();
            currentBomb = null;
        }
    }

    void HandleResetPosition()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 将玩家位置重置到初始位置
            transform.position = startPosition;
            rb.velocity = Vector3.zero; // 重置速度
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
    
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Define the ray originating from the camera
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                // Check if the hit object has the Interactable tag
                if (hit.collider.CompareTag("Interactable"))
                {
                    // Add the item to the interacted items list
                    interactedItems.Add(hit.collider.gameObject);

                    // Destroy the interactable object
                    Destroy(hit.collider.gameObject);

                    // Optional: Provide feedback to the player
                    Debug.Log("Interacted with: " + hit.collider.gameObject.name);
                }
            }
        }
    }
}
