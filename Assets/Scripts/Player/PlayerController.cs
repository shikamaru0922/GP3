using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject bombPrefab;          // 炸弹预制件
    public Transform bombSpawnPoint;       // 炸弹生成位置
    public float throwForce = 15f;         // 投掷力度
    public float moveSpeed = 5f;           // 移动速度
    public float maxspeed_inSpace = 30;
    public float mouseSensitivity = 2f;    // 鼠标灵敏度

    public Rigidbody rb;
    public bool isGrounded;
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

    [Header("Player Health Settings")]
    public int maxHealth = 3;    // 最大生命值
    public int currentHealth;   // 当前生命值
    public float landingSpeedThreshold = 22f; // 着陆速度阈值，超过此值则扣除生命值

    public Animator animator; // Animator 组件的引用

    [Header("Task Settings")]
    public int taskTargetCount = 0; // 已完成的任务目标数量

    [Header("Audio Settings")]
    public AudioSource movementAudioSource; // 用于播放走路音效
    public AudioSource landingAudioSource;  // 用于播放落地音效
    public AudioSource actionAudioSource;   // 用于播放其他动作音效
    public AudioClip walkingSound;          // 走路音效
    public AudioClip landingSound;          // 落地音效
    public AudioClip throwBombSound;        // 扔炸弹音效

    // 管理音效播放状态的字典
    private Dictionary<AudioClip, bool> audioClipPlayingStatus = new Dictionary<AudioClip, bool>();

    public String SceneName;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        Cursor.lockState = CursorLockMode.Locked;

        // 保存初始位置
        startPosition = transform.position;

        // 初始化生命值
        currentHealth = maxHealth;

        // 初始化音频组件
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 3)
        {
            movementAudioSource = audioSources[0];
            landingAudioSource = audioSources[1];
            actionAudioSource = audioSources[2];
        }
        else
        {
            Debug.LogError("需要在玩家对象上添加三个 AudioSource 组件！");
        }

        // 设置走路音效的 AudioSource
        movementAudioSource.clip = walkingSound;
        movementAudioSource.loop = true; // 设置为循环播放
        movementAudioSource.playOnAwake = false; // 不在 Awake 时自动播放

        // 设置落地音效的 AudioSource
        landingAudioSource.clip = landingSound;
        landingAudioSource.loop = false;
        landingAudioSource.playOnAwake = false;
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
        UpdateAnimatorStates();
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
        }
    }

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

        // 更新 Animator 参数
        bool isMoving = moveDirection.magnitude > 0.1f; // 当移动输入大于一定值时，认为玩家在移动
        animator.SetBool("isMoving", isMoving);

        // 控制走路音效的播放和停止
        if (isMoving && isGrounded)
        {
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play(); // 开始播放走路音效
                Debug.Log("开始播放走路音效");
            }
        }
        else
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop(); // 停止播放走路音效
                Debug.Log("停止播放走路音效");
            }
        }
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

                // 触发扔炸弹动画
                animator.SetTrigger("ThrowTrigger");

                // 播放扔炸弹音效（防止重复播放）
                if (!IsAudioClipPlaying(throwBombSound))
                {
                    actionAudioSource.PlayOneShot(throwBombSound);
                    SetAudioClipPlaying(throwBombSound, true);
                    StartCoroutine(ResetAudioClipPlayingStatus(throwBombSound));
                    Debug.Log("播放扔炸弹音效");
                }
                else
                {
                    Debug.Log("扔炸弹音效正在播放，跳过播放");
                }
            }
            else
            {
                // 可选：提供反馈，告知玩家处于冷却状态
                Debug.Log("无法投掷炸弹：处于冷却状态。");
            }
        }
    }

    // 音效播放状态管理方法
    bool IsAudioClipPlaying(AudioClip clip)
    {
        if (audioClipPlayingStatus.ContainsKey(clip))
        {
            return audioClipPlayingStatus[clip];
        }
        return false;
    }

    void SetAudioClipPlaying(AudioClip clip, bool isPlaying)
    {
        if (audioClipPlayingStatus.ContainsKey(clip))
        {
            audioClipPlayingStatus[clip] = isPlaying;
        }
        else
        {
            audioClipPlayingStatus.Add(clip, isPlaying);
        }
    }

    IEnumerator ResetAudioClipPlayingStatus(AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        SetAudioClipPlaying(clip, false);
    }

    void HandleBombDetonate()
    {
        if (Input.GetMouseButtonDown(1) && currentBomb != null)
        {
            animator.SetTrigger("BombTrigger");
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
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    taskTargetCount++;
                    Debug.Log("Current task target count: " + taskTargetCount);
                    

                    if (taskTargetCount >= 3)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        Debug.Log("Cursor unlocked");
                        SceneManager.LoadScene("Victory");
                    }

                    interactedItems.Add(hit.collider.gameObject);
                    Destroy(hit.collider.gameObject);

                    Debug.Log("Interacted with object: " + hit.collider.gameObject.name);
                }
            }
        }
    }

    void UpdateAnimatorStates()
    {
        // 更新 isGrounded 参数
        bool wasGrounded = animator.GetBool("isGrounded");
        animator.SetBool("isGrounded", isGrounded);

        // 更新 isFalling 参数
        bool isFalling = !isGrounded && gravityNum == 1;
        animator.SetBool("isFalling", isFalling);

        // 更新 isFlying 参数
        bool isFlying = gravityNum == 0;
        animator.SetBool("isFlying", isFlying);

        // 检测从下落到着陆的状态变化
        if (!wasGrounded && isGrounded)
        {
            // 触发着陆动画
            animator.SetTrigger("LandingTrigger");

            // 播放落地音效（如果未在播放）
            if (!landingAudioSource.isPlaying)
            {
                landingAudioSource.Play();
                Debug.Log("播放落地音效");
            }
            else
            {
                Debug.Log("落地音效正在播放，跳过播放");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // 玩家死亡，处理死亡逻辑
            Die();
        }
        else
        {
            // 可选：播放受伤动画或效果
            Debug.Log("玩家受伤，当前生命值：" + currentHealth);
        }
    }

    void Die()
    {
        // 禁用玩家控制
        this.enabled = false;

        // 禁用玩家的碰撞体和刚体（可选）
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        rb.isKinematic = true;

        // 显示死亡的 UI 占位符
        ShowDeathUI();

        // 输出日志
        Debug.Log("玩家死亡！");
    }

    void ShowDeathUI()
    {
        SceneManager.LoadScene(SceneName);
    }
}
