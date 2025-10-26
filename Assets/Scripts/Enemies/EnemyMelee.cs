using UnityEngine;

public class EnemyMelee : EnemyBase
{

    [Header("Patrol (Optional)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0f)] private float waypointReachEps = 0.1f;
    [SerializeField, Min(0f)] private float idleAtPointTime = 0.0f;

    private MeleeStats meleeStats;

    private int wpIndex;
    private float cooldown;
    private float idleTimer;

    private const float MIN_STOP_DISTANCE = 0.01f;

    protected override void Awake()
    {
        base.Awake();
        meleeStats = stats as MeleeStats;
        if (meleeStats == null)
        {
            Debug.LogError("EnemyMelee received wrong Stats SO! Make sure you assign a 'Melee Stats' file.", this);
        }
    }

    protected override void PatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0) { Stop(); return; }

        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
            Stop();

            if (animator) animator.SetBool("IsPatrolling", false);

            if (idleTimer <= 0f && animator)
                animator.SetBool("IsPatrolling", true);

            return;
        }

        var dest = (Vector2)waypoints[wpIndex].position;
        if (!CanStepTowardsX(dest))
        {
            Stop();
            return;
        }
        MoveTowards(dest);

        if (Vector2.Distance(transform.position, dest) <= waypointReachEps)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;

            if (idleAtPointTime > 0f)
            {
                idleTimer = idleAtPointTime;
                Stop();

                if (animator) animator.SetBool("IsPatrolling", false);
            }
        }
    }

    protected override void ChaseTick()
    {
        if (!target || meleeStats == null)
        {
            Stop();
            return;
        }

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 chasePos = new Vector2(targetPos.x, selfPos.y);

        if (!CanStepTowardsX(chasePos))
        {
            Stop();
            return;
        }

        float distX = Mathf.Abs(targetPos.x - selfPos.x);

        if (distX > Mathf.Max(meleeStats.stopDistance, MIN_STOP_DISTANCE))
            MoveTowards(chasePos);
        else
            Stop();
    }

    protected override void AttackTick()
    {
        if (!target || meleeStats == null)
        {
            Stop();
            return;
        }

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 lockedPos = new Vector2(targetPos.x, selfPos.y);

        if (!CanStepTowardsX(lockedPos))
        {
            Stop();
            return;
        }

        float distX = Mathf.Abs(targetPos.x - selfPos.x);
        if (distX > meleeStats.stopDistance)
            MoveTowards(lockedPos);
        else
            Stop();

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= meleeStats.meleeRange && target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(meleeStats.meleeDamage);
            cooldown = meleeStats.hitCooldown;
        }
    }
}
