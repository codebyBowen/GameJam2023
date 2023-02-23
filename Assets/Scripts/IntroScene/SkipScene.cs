using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipScene : MonoBehaviour
{
    public void SkipIntro() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }

    public void Retry() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }
    public void Quit() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
    public void SkipTransInfo() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SecondBossScene");
    }
    public void WatchCredits() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Credit");
    }
}
