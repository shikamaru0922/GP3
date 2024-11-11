using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject bombPrefab;          // 炸弹预制件
    public Transform bombSpawnPoint;       // 炸弹生成位置
    public float throwForce = 15f;         // 投掷力度
    public float moveSpeed = 5f;           // 移动速度
    public float airControlFactor = 0.5f;  // 空中控制系数
    public float mouseSensitivity = 2f;    // 鼠标灵敏度

    private Rigidbody rb;
    private bool isGrounded;
    private Bomb currentBomb;
    private float xRotation = 0f;

    private Vector3 startPosition;         // 初始位置

    public int gravityNum;
    public int playerScore;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        // 保存初始位置
        startPosition = transform.position;
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
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * h + transform.forward * v;

        if (isGrounded)
        {
            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + move);
        }
        else
        {
            rb.AddForce(moveDirection * moveSpeed * airControlFactor);
        }
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
