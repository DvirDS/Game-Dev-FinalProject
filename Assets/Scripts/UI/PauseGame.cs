using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public void OnResumeButton()
    {
        GameManager.I.ResumeGame();
        gameObject.SetActive(false); 
    }

    public void OnQuitToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}
