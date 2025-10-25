using UnityEngine;

/// <summary>
/// סקריפט זה מפעיל את מד החיים של הבוס כשהשחקן נכנס לטריגר,
/// ומסתיר אותו כשהשחקן יוצא.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossArenaTrigger : MonoBehaviour
{
    [SerializeField]
    private EnemyBase bossToTrack; // גרור לכאן את הבוס שלך מה-Hierarchy

    [SerializeField]
    private bool triggerOnce = false; // שיניתי ל-false כדי לאפשר כניסה ויציאה חוזרות

    private bool hasBeenTriggered = false;

    private void Awake()
    {
        // ודא שהקוליידר הוא טריגר
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("BossArenaTrigger needs a Collider2D component!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // בדוק אם מי שנכנס הוא השחקן ושהטריגר עדיין לא הופעל
        if (collision.CompareTag("Player") && (!triggerOnce || !hasBeenTriggered))
        {
            if (bossToTrack != null && UIManager.I != null)
            {
                hasBeenTriggered = true;

                // קרא ל-UIManager והעבר לו את הבוס הרלוונטי
                UIManager.I.ShowBossHealth(bossToTrack);
            }
            else
            {
                if (bossToTrack == null) Debug.LogWarning("BossArenaTrigger: 'Boss To Track' is not assigned!", this);
                if (UIManager.I == null) Debug.LogWarning("BossArenaTrigger: Could not find UIManager.I instance!", this);
            }
        }
    }

    // --- הוספנו את הפונקציה הזו ---
    private void OnTriggerExit2D(Collider2D collision)
    {
        // בדוק אם השחקן הוא זה שיצא
        if (collision.CompareTag("Player"))
        {
            // קרא ל-UIManager להסתיר את מד החיים
            if (UIManager.I != null)
            {
                UIManager.I.HideBossHealth();
            }
        }
    }
    // --- סוף החלק שנוסף ---
}