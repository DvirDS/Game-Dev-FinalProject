using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

    [Header("Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 4f;

    [Header("Detection Shape")]
    [SerializeField] protected bool useBoxDetection = true; // אם true – נשתמש במלבן
    [SerializeField] protected Vector2 detectionBoxSize = new Vector2(8f, 4f);
    [SerializeField] protected Vector2 attackBoxSize = new Vector2(4f, 2f);

    [Header("Line of Sight")]
    [Tooltip("Layers that block enemy vision (e.g. Walls, Ground)")]
    [SerializeField] protected LayerMask obstacleMask;

    [Header("Refs")]
    [SerializeField] protected Transform target;
    protected Rigidbody2D rb;

    [SerializeField] protected EnemyState state = EnemyState.Patrol;

    [Header("Health")]
    [SerializeField, Min(1)] protected int maxHealth = 20;
    [SerializeField, Min(0f)] protected float hurtDuration = 0.2f;
    [SerializeField, Min(0f)] protected float deathDestroyDelay = 1.5f;

    protected int _health;
    protected bool _invulnerable;

    [Header("Animator")]
    [SerializeField] protected Animator animator;

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool flipByVelocity = true;
    [SerializeField] protected bool facingRightDefault = true;

    // ======== NEW: Tilemap-ground edge/wall checks ========
    [Header("Ground / Edges (Tilemap)")]
    [SerializeField] protected LayerMask groundMask;            // Layer של ה-Tilemap (Ground)
    [SerializeField] protected Vector2 feetOffset = new Vector2(0f, -0.5f);
    [SerializeField] protected float edgeCheckForward = 0.45f;  // כמה קדימה לבדוק רצפה
    [SerializeField] protected float edgeCheckDown = 0.9f;      // עומק הבדיקה למטה
    [SerializeField] protected float wallCheckDistance = 0.25f; // מרחק בדיקת קיר/עמוד קדימה

    // Animator hash references
    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashIsPatrolling = Animator.StringToHash("IsPatrolling");
    static readonly int HashIsChasing = Animator.StringToHash("IsChasing");
    static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    static readonly int HashHurt = Animator.StringToHash("Hurt");
    static readonly int HashIsDead = Animator.StringToHash("IsDead");

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.freezeRotation = true;

        _health = Mathf.Max(1, maxHealth);

        if (!target)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        OnEnter(state);
    }

    protected virtual void Update()
    {
        if (animator)
            animator.SetFloat(HashSpeed, Mathf.Abs(rb ? rb.linearVelocity.x : 0f));

        UpdateFacingByVelocity();

        if (state == EnemyState.Dead) { Stop(); return; }

        if (state == EnemyState.Hurt)
        {
            Debug.Log("Update loop is correctly returning because state is Hurt.");
            return;
        }

        if (GameManager.I &&
            (GameManager.I.State == GameManager.GameState.Pause ||
             GameManager.I.State == GameManager.GameState.Dialogue ||
             GameManager.I.State == GameManager.GameState.GameOver))
        {
            Stop();
            return;
        }

        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);

        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Attack: AttackTick(); break;
        }
    }

    // ====== זיהוי טווח עם קו ראייה ======
    protected bool InRange(float range)
    {
        if (!target) return false;

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;

        // בדיקה אם השחקן בכלל בטווח הצורה
        bool insideRange = false;

        if (!useBoxDetection)
        {
            insideRange = Vector2.Distance(selfPos, targetPos) <= range;
        }
        else
        {
            Vector2 size = (range == attackRange) ? attackBoxSize : detectionBoxSize;
            Vector2 half = size * 0.5f;
            insideRange =
                (targetPos.x >= selfPos.x - half.x && targetPos.x <= selfPos.x + half.x &&
                 targetPos.y >= selfPos.y - half.y && targetPos.y <= selfPos.y + half.y);
        }

        if (!insideRange) return false;

        // בדיקה אם יש קיר בין האויב לשחקן
        RaycastHit2D hit = Physics2D.Linecast(selfPos, targetPos, obstacleMask);
        if (hit.collider != null)
        {
            // אם יש אובייקט חוסם ראייה
            return false;
        }

        return true;
    }

    protected void MoveTowards(Vector2 pos)
    {
        Vector2 dir = (pos - (Vector2)transform.position);
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
        if (rb) rb.linearVelocity = dir * moveSpeed;
    }

    protected void Stop()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    protected void ChangeState(EnemyState next)
    {
        if (state == next) return;
        OnExit(state);
        state = next;
        OnEnter(state);
    }

    protected virtual void OnEnter(EnemyState s)
    {
        switch (s)
        {
            case EnemyState.Patrol:
                if (animator)
                {
                    animator.SetBool(HashIsPatrolling, true);
                    animator.SetBool(HashIsChasing, false);
                    animator.SetBool(HashIsAttacking, false);
                }
                break;

            case EnemyState.Chase:
                if (animator)
                {
                    animator.SetBool(HashIsChasing, true);
                    animator.SetBool(HashIsPatrolling, false);
                    animator.SetBool(HashIsAttacking, false);
                }
                break;

            case EnemyState.Attack:
                if (animator) animator.SetBool(HashIsAttacking, true);
                break;

            case EnemyState.Hurt:
                Debug.Log("--- STEP 3: OnEnter(Hurt) was called. Setting animation trigger. ---");
                //Stop();
                if (animator) animator.SetTrigger(HashHurt);
                break;

            case EnemyState.Dead:
                Stop();
                if (animator) animator.SetBool(HashIsDead, true);
                break;
        }
    }

    protected virtual void OnExit(EnemyState s)
    {
        if (s != EnemyState.Attack) Stop();

        switch (s)
        {
            case EnemyState.Patrol:
                if (animator) animator.SetBool(HashIsPatrolling, false);
                break;
            case EnemyState.Chase:
                if (animator) animator.SetBool(HashIsChasing, false);
                break;
            case EnemyState.Attack:
                if (animator) animator.SetBool(HashIsAttacking, false);
                break;
        }
    }

    protected abstract void PatrolTick();
    protected virtual void ChaseTick() { if (target) MoveTowards(target.position); }
    protected abstract void AttackTick();

    public void Kill()
    {
        // אם האויב כבר בתהליך מוות, אל תתחיל אותו שוב
        if (state == EnemyState.Dead) return;

        // קרא ישירות לקורוטינה שמפעילה את אנימציית המוות וההשמדה
        StartCoroutine(DieRoutine());
    }

    public virtual void TakeDamage(int amount)
    {
        if (state == EnemyState.Dead || _invulnerable)
        {
            Debug.LogWarning("TakeDamage BLOCKED. State: " + state + ", IsInvulnerable: " + _invulnerable);
            return;
        }

        Debug.Log("--- STEP 1: TakeDamage Called ---");
        _health -= Mathf.Max(1, amount);

        if (_health <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        _invulnerable = true;
        ChangeState(EnemyState.Hurt);
        Debug.Log("--- STEP 2: State changed to Hurt. Starting HurtRoutine. ---");
        StartCoroutine(HurtEndRoutine());
    }

    IEnumerator HurtEndRoutine()
    {
        Debug.Log("--- STEP 4: HurtEndRoutine has started, waiting for " + hurtDuration + " seconds. ---");
        yield return new WaitForSeconds(hurtDuration);
        Debug.Log("--- STEP 5: HurtEndRoutine has finished waiting. Resetting state. ---");

        _invulnerable = false;
        if (state == EnemyState.Hurt)
        {
            if (InRange(attackRange)) ChangeState(EnemyState.Attack);
            else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
            else ChangeState(EnemyState.Patrol);
        }
    }

    IEnumerator DieRoutine()
    {
        Debug.Log("DieRoutine has been called for: " + gameObject.name);
        ChangeState(EnemyState.Dead);
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }

    void UpdateFacingByVelocity()
    {
        if (!flipByVelocity || rb == null || !spriteRenderer) return;

        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) < 0.01f) return;

        bool movingRight = vx > 0f;
        spriteRenderer.flipX = movingRight;
    }

    void OnDrawGizmosSelected()
    {
        if (useBoxDetection)
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);
            Gizmos.DrawWireCube(transform.position, detectionBoxSize);

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Gizmos.DrawWireCube(transform.position, attackBoxSize);
        }
        else
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    // ===================== Helpers for Tilemap-based movement safety =====================
    /// <summary>החזרת סימן הכיוון לאובייקט יעד על ציר X</summary>
    protected int HorizontalDirTo(Vector2 targetPos)
    {
        float dx = targetPos.x - transform.position.x;
        if (dx > 0.01f) return 1;
        if (dx < -0.01f) return -1;
        return 0;
    }

    /// <summary>האם יש קיר/עמוד קדימה (על obstacleMask)?</summary>
    protected bool HasWallAhead(int dirSign)
    {
        if (dirSign == 0) return false;
        Vector2 origin = (Vector2)transform.position + feetOffset + new Vector2(dirSign * 0.1f, 0.1f);
        return Physics2D.Raycast(origin, new Vector2(dirSign, 0f), wallCheckDistance, obstacleMask);
    }

    /// <summary>האם יש רצפה מתחת לנקודת הדריכה הבאה על ה-Tilemap (groundMask)?</summary>
    protected bool HasGroundAhead(int dirSign)
    {
        if (dirSign == 0) return true;
        Vector2 origin = (Vector2)transform.position + feetOffset + Vector2.right * (dirSign * edgeCheckForward);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDown, groundMask);
        return hit.collider != null;
    }

    /// <summary>אפשר להתקדם צעד על ציר X מבלי ליפול/להיתקע?</summary>
    protected bool CanStepTowardsX(Vector2 targetPos)
    {
        int s = HorizontalDirTo(targetPos);
        if (s == 0) return true;
        if (HasWallAhead(s)) return false;    // קיר/עמוד לפני האויב
        if (!HasGroundAhead(s)) return false; // אין רצפה בהמשך — קצה פלטפורמה
        return true;
    }

}
