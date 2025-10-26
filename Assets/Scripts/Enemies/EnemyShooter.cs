using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private Transform muzzle;

    private ShooterStats shooterStats;
    private float cd;

    [Header("Patrol (Optional)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0f)] private float waypointReachEps = 0.1f;
    [SerializeField, Min(0f)] private float idleAtPointTime = 0.0f;

    private int wpIndex;
    private float idleTimer;

    private const float MIN_STOP_DISTANCE = 0.01f;

    protected override void Awake()
    {
        base.Awake(); // קורא ל-Awake של EnemyBase (שמגדיר חיים וכו')

        // --- זה הקסם ---
        // אנו מניחים שה-stats שקיבלנו הוא מסוג ShooterStats
        shooterStats = stats as ShooterStats;
        if (shooterStats == null)
        {
            Debug.LogError("EnemyShooter received wrong Stats SO!", this);
        }
        // --- סוף הקסם ---
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
        if (!target)
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
        if (distX > Mathf.Max(shooterStats.stopDistance, MIN_STOP_DISTANCE))
            MoveTowards(chasePos);
        else
            Stop();
    }

    protected override void AttackTick()
    {
        if (!target || shooterStats == null)
        {
            Stop();
            return;
        }

        FaceTarget();

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 lockedPos = new Vector2(targetPos.x, selfPos.y);

        if (!CanStepTowardsX(lockedPos))
        {
            Stop();
        }
        else
        {
            float distX = Mathf.Abs(targetPos.x - selfPos.x);
            if (distX > shooterStats.stopDistance)
                MoveTowards(lockedPos);
            else
                Stop();
        }

        cd -= Time.deltaTime;
        if (cd <= 0f && shooterStats.projectilePrefab && muzzle)
        {
            var go = Instantiate(shooterStats.projectilePrefab, muzzle.position, muzzle.rotation);
            if (go.TryGetComponent<Projectile>(out var proj))
            {
                proj.ownerTag = "Enemy";
                proj.damage = shooterStats.damage;
                proj.speed = shooterStats.projectileSpeed;
                var dir = (target.position - muzzle.position).normalized;
                proj.Launch(dir);
            }
            cd = 1f / Mathf.Max(shooterStats.shotsPerSecond, MIN_STOP_DISTANCE);
        }
    }

    void FaceTarget()
    {
        if (!spriteRenderer || !target) return;
        spriteRenderer.flipX = (target.position.x < transform.position.x);

    }
}
