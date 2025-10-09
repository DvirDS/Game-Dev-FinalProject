using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public void OnResumeButton()
    {
        GameManager.I.ResumeGame();
        gameObject.SetActive(false); // מסתיר את תפריט ה־Pause עצמו
    }

    public void OnQuitToMenu()
    {
        // מחזיר את הזמן
        Time.timeScale = 1f;
        // טוען את סצנת התפריט הראשי (אם יש לך כזו בשם "StartMenu")
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}
