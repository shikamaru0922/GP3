using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // ȷ����Ϸ��������������ȷ
        string gameSceneName = "opening"; // ��Ϸ������������
        SceneManager.LoadScene(gameSceneName);
    }
}