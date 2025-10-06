using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel; // גרור לכאן את הפאנל של הקרדיטים דרך האינספקטור

    // כפתור "התחל משחק" - טוען את סצנת השלב הראשון
    public void PlayGame()
    {
        // אפשר לפי שם:
        SceneManager.LoadScene("Game");
        // או לפי אינדקס אם השלב הראשון הוא 1:
        // SceneManager.LoadScene(1);
    }

    // כפתור "קרדיטים" - מציג את הפאנל
    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    // כפתור "חזרה" בתוך הקרדיטים - מסתיר את הפאנל
    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    // כפתור "יציאה"
    public void QuitGame()
    {
        // בסביבת העריכה זה פשוט יעצור את מצב ה-Play
        UnityEditor.EditorApplication.isPlaying = false;
        // בבילד אמיתי זה יסגור את המשחק
        Application.Quit();
    }
}
