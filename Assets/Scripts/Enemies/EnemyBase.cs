using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    // ������ Hurt �-Dead
    public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

    [Header("Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 4f;

    [Header("Refs")]
    [SerializeField] protected Transform target;

    protected Rigidbody2D rb;
    [SerializeField] protected EnemyState state = EnemyState.Patrol;

    // === Health (����� ��������) ===
    [Header("Health")]
    [SerializeField, Min(1)] protected int maxHealth = 20;
    [SerializeField, Min(0f)] protected float hurtDuration = 0.2f;      // ��� Hurt ��� (i-frames)
    [SerializeField, Min(0f)] protected float deathDestroyDelay = 1.5f; // ����� ���� Destroy

    protected int _health;
    protected bool _invulnerable;

    // === ��������� ===
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        _health = Mathf.Max(1, maxHealth);

        // �� �� ���� ���, ��� ����� �� �����
        if (!target)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
    }

    protected virtual void Update()
    {
        // �� �� � �� ������ ������
        if (state == EnemyState.Dead) { Stop(); return; }

        // ����/������/��������� � ����� AI (��� ���� ����)
        if (GameManager.I && (GameManager.I.State == GameManager.GameState.Pause ||
                              GameManager.I.State == GameManager.GameState.Dialogue ||
                              GameManager.I.State == GameManager.GameState.GameOver))
        {
            Stop();
            return;
        }

        // ���� Hurt ������ ������ ������ ��� ������ �����
        if (state == EnemyState.Hurt)
        {
            return;
        }

        // Transitions �� ����� (��� ����)
        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);

        // ������� ��� State (������ �� case-�� ����� �-Hurt/Dead)
        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Attack: AttackTick(); break;
            case EnemyState.Hurt: break;
            case EnemyState.Dead: return;
        }
    }

    // === API ����� ����� ����� ===
    protected bool InRange(float r) =>
        target && Vector2.Distance(transform.position, target.position) <= r;

    protected void MoveTowards(Vector2 pos)
    {
        Vector2 dir = (pos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed; // ���� ��� ����
    }

    protected void Stop() => rb.linearVelocity = Vector2.zero;

    // === ����� ��� ���� ===
    protected virtual void OnEnter(EnemyState s)
    {
        if (s == EnemyState.Hurt)
        {
            Stop();
            // �� ���� �����: animator?.SetTrigger("Hurt");
        }
        else if (s == EnemyState.Dead)
        {
            Stop();
            // �� ���� �����: animator?.SetBool("IsDead", true);
            // ���� �� ������ ��������� ��� �� ����.
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

    // === ������ ����� ������� ���� ���� ===
    protected abstract void PatrolTick();
    protected virtual void ChaseTick() { if (target) MoveTowards(target.position); }
    protected abstract void AttackTick();

    // === Damage / Hurt / Death � �������, �� ���� ������ ===
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

        // ���� ����� ���� �����
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
