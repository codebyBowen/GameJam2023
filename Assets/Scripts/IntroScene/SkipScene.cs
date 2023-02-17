using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipScene : MonoBehaviour
{
    public void SkipIntro() {
        Debug.Log("MainGameScene?");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }
}
