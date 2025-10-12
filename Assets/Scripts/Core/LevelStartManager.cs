using UnityEngine;

public class LevelStartManager : MonoBehaviour
{
    void Start()
    {
        // בצע את הפעולה רק אם הגענו לכאן מהתפריט הראשי
        if (GameManager.I != null && GameManager.I.State == GameManager.GameState.MainMenu)
        {
            GameManager.I.StartGame();
        }
    }
}