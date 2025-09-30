using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    // הוספנו Hurt ו-Dead
    public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

    [Header("Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 4f;

    [Header("Refs")]
    [SerializeField] protected Transform target;

    protected Rigidbody2D rb;
    [SerializeField] protected EnemyState state = EnemyState.Patrol;

    // === Health (תוספת מינימלית) ===
    [Header("Health")]
    [SerializeField, Min(1)] protected int maxHealth = 20;
    [SerializeField, Min(0f)] protected float hurtDuration = 0.2f;      // זמן Hurt קצר (i-frames)
    [SerializeField, Min(0f)] protected float deathDestroyDelay = 1.5f; // השהיה לפני Destroy

    protected int _health;
    protected bool _invulnerable;

    // === לייפסייקל ===
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        _health = Mathf.Max(1, maxHealth);

        // אם לא שובץ יעד, נסה למצוא את השחקן
        if (!target)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
    }

    protected virtual void Update()
    {
        // אם מת — לא מבצעים לוגיקה
        if (state == EnemyState.Dead) { Stop(); return; }

        // פאוז/דיאלוג/גיים־אובר — עצירת AI (כמו שהיה אצלך)
        if (GameManager.I && (GameManager.I.State == GameManager.GameState.Pause ||
                              GameManager.I.State == GameManager.GameState.Dialogue ||
                              GameManager.I.State == GameManager.GameState.GameOver))
        {
            Stop();
            return;
        }

        // בזמן Hurt נותנים לפגיעה “לרוץ” בלי להחליף מצבים
        if (state == EnemyState.Hurt)
        {
            return;
        }

        // Transitions של האויב (כמו שהיה)
        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);

        // התנהגות לפי State (הוספנו רק case-ים ריקים ל-Hurt/Dead)
        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Attack: AttackTick(); break;
            case EnemyState.Hurt: break;
            case EnemyState.Dead: return;
        }
    }

    // === API משותף ועזרי תנועה ===
    protected bool InRange(float r) =>
        target && Vector2.Distance(transform.position, target.position) <= r;

    protected void MoveTowards(Vector2 pos)
    {
        Vector2 dir = (pos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed; // נשאר כפי שהיה
    }

    protected void Stop() => rb.linearVelocity = Vector2.zero;

    // === שינוי מצב קליל ===
    protected virtual void OnEnter(EnemyState s)
    {
        if (s == EnemyState.Hurt)
        {
            Stop();
            // אם תרצה בעתיד: animator?.SetTrigger("Hurt");
        }
        else if (s == EnemyState.Dead)
        {
            Stop();
            // אם תרצה בעתיד: animator?.SetBool("IsDead", true);
            // אפשר גם להשבית קוליידרים כאן אם צריך.
        }
    }

    protected virtual void OnExit(EnemyState s)
    {
        if (s != EnemyState.Attack) Stop();
    }

    protected void ChangeState(EnemyState next)
    {
        if (state == next) return;
        OnExit(state);
        state = next;
        OnEnter(state);
    }

    // === נקודות הרחבה שחייבים לממש בילד ===
    protected abstract void PatrolTick();
    protected virtual void ChaseTick() { if (target) MoveTowards(target.position); }
    protected abstract void AttackTick();

    // === Damage / Hurt / Death – מינימלי, לא נוגע בילדים ===
    public virtual void TakeDamage(int amount)
    {
        if (state == EnemyState.Dead || _invulnerable) return;

        _health -= Mathf.Max(1, amount);
        if (_health <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        _invulnerable = true;
        ChangeState(EnemyState.Hurt);

        yield return new WaitForSeconds(hurtDuration);

        _invulnerable = false;

        // חזרה “חכמה” למצב מתאים
        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);
    }

    private IEnumerator DieRoutine()
    {
        ChangeState(EnemyState.Dead);
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }
}
