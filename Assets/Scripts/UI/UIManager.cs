using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText; 
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI shopScoreText;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Screens")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel; 

    void OnEnable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnPlayerHealthChanged += UpdateHealth;
            GameManager.I.OnStateChanged += HandleGameStateChanged;
            GameManager.I.OnScoreChanged += UpdateScore;
            GameManager.I.OnAmmoChanged += UpdateAmmo;
            UpdateScore(GameManager.I.Score);
        }
    }

    void OnDisable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnPlayerHealthChanged -= UpdateHealth;
            GameManager.I.OnStateChanged -= HandleGameStateChanged;
            GameManager.I.OnScoreChanged -= UpdateScore;
            GameManager.I.OnAmmoChanged -= UpdateAmmo;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState s)
    {
        bool inPlay = (s == GameManager.GameState.Play);
        if (hudRoot) hudRoot.SetActive(inPlay);
        if (pausePanel) pausePanel.SetActive(s == GameManager.GameState.Pause);
        if (gameOverPanel) gameOverPanel.SetActive(s == GameManager.GameState.GameOver);
        if (victoryPanel) victoryPanel.SetActive(s == GameManager.GameState.Victory); 

        if (inPlay)
        {
            UpdateScore(GameManager.I.Score);
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar)
        {
            healthBar.maxValue = Mathf.Max(1, max);
            healthBar.value = Mathf.Clamp(current, 0, max);
        }

        if (healthText) healthText.text = $"{current} / {max}";
    }

    private void UpdateScore(int newScore)
    {
        string scoreString = "Score: " + newScore;

        // Update the main HUD score
        if (scoreText)
        {
            scoreText.text = scoreString;
        }

        // Also update the shop score
        if (shopScoreText)
        {
            shopScoreText.text = scoreString;
        }
    }

    private void UpdateAmmo(int currentAmmo)
    {
        if (ammoText)
        {
            if (currentAmmo == -1) // -1 is our signal for infinite ammo
            {
                ammoText.text = "Ammo: --";
            }
            else
            {
                ammoText.text = "Ammo: " + currentAmmo;
            }
        }
    }

    public void OnResumeClicked() => GameManager.I?.ResumeGame();

    public void OnReturnToMainMenuClicked()
    {
        GameManager.I?.ReturnToMainMenu();
    }
}
