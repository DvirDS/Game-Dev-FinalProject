using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject hudRoot;

    [Header("Screens")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    void OnEnable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnPlayerHealthChanged += UpdateHealth;
            GameManager.I.OnStateChanged += HandleGameStateChanged;
        }
    }

    void OnDisable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnPlayerHealthChanged -= UpdateHealth;
            GameManager.I.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState s)
    {
        bool inPlay = (s == GameManager.GameState.Play);
        if (hudRoot) hudRoot.SetActive(inPlay);
        if (pausePanel) pausePanel.SetActive(s == GameManager.GameState.Pause);
        if (gameOverPanel) gameOverPanel.SetActive(s == GameManager.GameState.GameOver);
    }

    public void UpdateHealth(int current, int max)
    {
        if (!healthBar) return;
        healthBar.maxValue = Mathf.Max(1, max);
        healthBar.value = Mathf.Clamp(current, 0, max);
    }

    // כפתור Resume בפאנל Pause (אם יש)
    public void OnResumeClicked() => GameManager.I?.ResumeGame();
}
