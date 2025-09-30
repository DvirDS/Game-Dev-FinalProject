using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 4f;

    [Header("Refs")]
    [SerializeField] protected Transform target;

    protected Rigidbody2D rb;
    [SerializeField] protected EnemyState state = EnemyState.Patrol;

    // === ��������� ===
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    protected virtual void Update()
    {
        // �� ����� �����/������ � ����� AI (������ ������ ����� �� "���� �����/������ �� ����� �����")
        if (GameManager.I && (GameManager.I.State == GameManager.GameState.Pause ||
                              GameManager.I.State == GameManager.GameState.Dialogue ||
                              GameManager.I.State == GameManager.GameState.GameOver))
        {
            Stop();
            return;
        }

        // Transitions �� �����
        if (InRange(attackRange)) ChangeState(EnemyState.Attack);
        else if (InRange(detectionRange)) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Patrol);

        // ������� ��� State
        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Attack: AttackTick(); break;
        }
    }

    // === API ����� ����� ����� ===
    protected bool InRange(float r) =>
        target && Vector2.Distance(transform.position, target.position) <= r;

    protected void MoveTowards(Vector2 pos)
    {
        Vector2 dir = (pos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    protected void Stop() => rb.linearVelocity = Vector2.zero;

    // === ����� ��� ���� ===
    protected virtual void OnEnter(EnemyState s) { }
    protected virtual void OnExit(EnemyState s) { if (s != EnemyState.Attack) Stop(); }

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
}
