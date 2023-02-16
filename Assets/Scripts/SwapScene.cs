using UnityEngine;
using UnityEngine.SceneManagement;

public class SwapScene : MonoBehaviour
{
    public void PlayGame() {
        // SceneManager.loadScene(1)
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
