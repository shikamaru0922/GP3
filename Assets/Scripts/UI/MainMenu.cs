using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // 确保游戏主场景的名称正确
        string gameSceneName = "1"; // 游戏主场景的名称
        SceneManager.LoadScene(gameSceneName);
    }
}