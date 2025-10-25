using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossArenaTrigger : MonoBehaviour
{
    [SerializeField]
    private EnemyBase bossToTrack;

    [SerializeField]
    private bool triggerOnce = false;

    private bool hasBeenTriggered = false;

    private void Awake()
    {
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
        if (collision.CompareTag("Player") && (!triggerOnce || !hasBeenTriggered))
        {
            if (bossToTrack != null && UIManager.I != null)
            {
                hasBeenTriggered = true;

                UIManager.I.ShowBossHealth(bossToTrack);
            }
            else
            {
                if (bossToTrack == null) Debug.LogWarning("BossArenaTrigger: 'Boss To Track' is not assigned!", this);
                if (UIManager.I == null) Debug.LogWarning("BossArenaTrigger: Could not find UIManager.I instance!", this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (UIManager.I != null)
            {
                UIManager.I.HideBossHealth();
            }
        }
    }
}