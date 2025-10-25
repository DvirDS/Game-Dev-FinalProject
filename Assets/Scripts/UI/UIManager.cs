using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager I { get; private set; }


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

    [Header("Boss")]
    [SerializeField] private GameObject bossHealthBarRoot;
    [SerializeField] private Slider bossHealthSlider;
    private EnemyBase currentBoss;


    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else if (I != this)
        {
            Debug.LogWarning("Multiple UIManagers found, destroying this one.");
            Destroy(gameObject);
        }
    }

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

    void Start()
    {
        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(false);
        }
        GameManager.I?.SetState(GameManager.GameState.Play);
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

        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealth;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState s)
    {
        bool inPlay = (s == GameManager.GameState.Play);
        if (hudRoot) hudRoot.SetActive(inPlay);
        if (pausePanel) pausePanel.SetActive(s == GameManager.GameState.Pause);
        if (gameOverPanel) gameOverPanel.SetActive(s == GameManager.GameState.GameOver);
        if (victoryPanel) victoryPanel.SetActive(s == GameManager.GameState.Victory);
        if (s == GameManager.GameState.GameOver || s == GameManager.GameState.Victory)
        {
            HideBossHealth();
        }
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

        if (scoreText)
        {
            scoreText.text = scoreString;
        }

        if (shopScoreText)
        {
            shopScoreText.text = scoreString;
        }
    }

    private void UpdateAmmo(int currentAmmo)
    {
        if (ammoText)
        {
            if (currentAmmo == -1)
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

    public void ShowBossHealth(EnemyBase boss)
    {
        if (boss == null)
        {
            Debug.LogError("ShowBossHealth called with a null boss.");
            return;
        }

        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealth;
        }

        currentBoss = boss;

        currentBoss.OnHealthChanged += UpdateBossHealth;

        UpdateBossHealth(currentBoss.GetCurrentHealth(), currentBoss.GetMaxHealth());

        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(true);
        }
    }

    private void UpdateBossHealth(int current, int max)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = Mathf.Max(1, max);
            bossHealthSlider.value = Mathf.Clamp(current, 0, max);
        }

        if (current <= 0)
        {
            HideBossHealth();
        }
    }

    public void HideBossHealth()
    {
        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(false);
        }

        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealth;
            currentBoss = null;
        }
    }

}
