using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

    [Header("Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 4f;

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

    // Hashes
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
        OnEnter(state);
    }

    protected virtual void Update()
    {
        if (animator)
            animator.SetFloat(HashSpeed, Mathf.Abs(rb ? rb.linearVelocity.x : 0f));

        if (state == EnemyState.Dead) { Stop(); return; }

        if (GameManager.I &&
            (GameManager.I.State == GameManager.GameState.Pause ||
             GameManager.I.State == GameManager.GameState.Dialogue ||
             GameManager.I.State == GameManager.GameState.GameOver))
        {
            Stop();
            return;
        }

        if (state == EnemyState.Hurt) return;

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

    protected bool InRange(float r) =>
        target && Vector2.Distance(transform.position, target.position) <= r;

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
                Stop();
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

    IEnumerator HurtRoutine()
    {
        _invulnerable = true;
        ChangeState(EnemyState.Hurt);
        yield return new WaitForSeconds(hurtDuration);
        _invulnerable = false;

        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);
    }

    IEnumerator DieRoutine()
    {
        ChangeState(EnemyState.Dead);
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }
}
