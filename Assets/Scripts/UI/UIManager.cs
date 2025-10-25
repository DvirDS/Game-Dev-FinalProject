using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    // --- הוספנו Singleton ---
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
    [SerializeField] private GameObject bossHealthBarRoot; // האובייקט הראשי שמכיל את הסליידר
    [SerializeField] private Slider bossHealthSlider;      // ה-Slider עצמו
    private EnemyBase currentBoss; // הפניה לבוס שאנחנו עוקבים אחריו


    void Awake()
    {
        // הגדרת ה-Singleton
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
        // ודא שמד החיים של הבוס מוסתר בהתחלה
        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(false);
        }

        // --- הוסף את השורה הבאה ---
        // עכשיו, כשה-UI מוכן והסצנה נטענה, אמור למנהל המשחק להיכנס למצב 'Play'
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
        // --- סוף ---
    }

    private void HandleGameStateChanged(GameManager.GameState s)
    {
        bool inPlay = (s == GameManager.GameState.Play);
        if (hudRoot) hudRoot.SetActive(inPlay);
        if (pausePanel) pausePanel.SetActive(s == GameManager.GameState.Pause);
        if (gameOverPanel) gameOverPanel.SetActive(s == GameManager.GameState.GameOver);
        if (victoryPanel) victoryPanel.SetActive(s == GameManager.GameState.Victory);
        // --- הוספנו את החלק הבא ---
        // הסתר את מד החיים של הבוס בסיום משחק או ניצחון
        if (s == GameManager.GameState.GameOver || s == GameManager.GameState.Victory)
        {
            HideBossHealth();
        }
        // --- סוף ---
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

    // --- הוספנו את 3 הפונקציות הבאות בסוף הקובץ ---

    /// <summary>
    /// פונקציה זו נקראת ע"י הטריגר של הזירה.
    /// היא מציגה את מד החיים ונרשמת לאירועים של הבוס.
    /// </summary>
    public void ShowBossHealth(EnemyBase boss)
    {
        if (boss == null)
        {
            Debug.LogError("ShowBossHealth called with a null boss.");
            return;
        }

        // אם אנחנו כבר עוקבים אחרי בוס (למקרה שיש כמה), נתק קודם
        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealth;
        }

        // שמור את הבוס החדש
        currentBoss = boss;

        // הירשם לאירוע שלו
        currentBoss.OnHealthChanged += UpdateBossHealth;

        // עדכן את מד החיים בפעם הראשונה (עם החיים המלאים שלו)
        UpdateBossHealth(currentBoss.GetCurrentHealth(), currentBoss.GetMaxHealth());

        // הצג את מד החיים
        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(true);
        }
    }

    /// <summary>
    /// פונקציה זו מוסתרת (private) ומופעלת רק ע"י האירוע של הבוס.
    /// </summary>
    private void UpdateBossHealth(int current, int max)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = Mathf.Max(1, max);
            bossHealthSlider.value = Mathf.Clamp(current, 0, max);
        }

        // אם הבוס מת, הסתר את המד
        if (current <= 0)
        {
            HideBossHealth();
        }
    }

    /// <summary>
    /// פונקציה להסתרה וניקוי
    /// </summary>
    public void HideBossHealth()
    {
        if (bossHealthBarRoot != null)
        {
            bossHealthBarRoot.SetActive(false);
        }

        // נתק את ההאזנה כדי למנוע דליפות זיכרון
        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealth;
            currentBoss = null;
        }
    }

}
