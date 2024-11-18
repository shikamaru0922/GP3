using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public float totalTime = 60f; // 总时间（秒）
    private float remainingTime;

    public Text timerText; // 显示计时器的 UI 文本

    public GameObject gameOverUI; // 游戏结束的 UI 界面
    public PlayerController playerController; // 玩家控制脚本引用

    private bool isGameOver = false;

    void Start()
    {
        remainingTime = totalTime;

        if (timerText == null)
        {
            timerText = GameObject.Find("TimerText").GetComponent<Text>();
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isGameOver = true;
                GameOver();
            }
            
        }
        // 添加以下代码，检测玩家是否按下 R 键
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        // 禁用玩家控制
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 显示游戏结束的 UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 暂停游戏时间
        Time.timeScale = 0f;

        // 输出日志信息
        Debug.Log("游戏结束！");
    }

    // 重新开始游戏的方法，可在游戏结束 UI 的按钮上绑定
    public void RestartGame()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}