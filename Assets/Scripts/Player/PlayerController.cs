using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject bombPrefab;          // 炸弹预制件
    public Transform bombSpawnPoint;       // 炸弹生成位置
    public float throwForce = 15f;         // 投掷力度
    public float moveSpeed = 5f;           // 移动速度
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
    
    [Header("Points")]
    public Vector3 startPoint;        // 起点（通常为当前 GameObject 的位置）
    public Vector3 endPoint;          // 初始方向的终点
    public Vector3 standtargetAngel;  // 目标方向的终点

    [Header("Rotation Settings")]
    public float rotationSpeed = 1f;  // 旋转速度（度/秒）

    private bool isRotating = false;  // 标记旋转是否正在进行
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
    }

    void FixedUpdate()
    {
        MovePlayer();
        AnimateAngle();
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
            GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);
            Rigidbody bombRb = bomb.GetComponent<Rigidbody>();

            bombRb.velocity = rb.velocity;

            Vector3 throwDirection = Camera.main.transform.forward;
            bombRb.AddForce(throwDirection.normalized * throwForce, ForceMode.VelocityChange);

            currentBomb = bomb.GetComponent<Bomb>();
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
}
