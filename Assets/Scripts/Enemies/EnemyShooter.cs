using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float shotsPerSecond = 1.5f;
    [SerializeField] private int damage = 8;
    [SerializeField] private float projectileSpeed = 10f;
    // מרחק עצירה מהשחקן, בדיוק כמו ב-Melee
    [SerializeField, Min(0f)] private float stopDistance = 3f;
    private float cd;

    [Header("Patrol (Optional)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0f)] private float waypointReachEps = 0.1f;
    [SerializeField, Min(0f)] private float idleAtPointTime = 0.0f;

    private int _wpIndex;
    private float _idleTimer;

    // פטרול - ללא שינוי
    protected override void PatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0) { Stop(); return; }
        if (_idleTimer > 0f)
        {
            _idleTimer -= Time.deltaTime;
            Stop();
            if (animator) animator.SetBool("IsPatrolling", false);
            if (_idleTimer <= 0f && animator)
                animator.SetBool("IsPatrolling", true);
            return;
        }
        var dest = (Vector2)waypoints[_wpIndex].position;
        if (!CanStepTowardsX(dest))
        {
            Stop();
            return;
        }
        MoveTowards(dest);
        if (Vector2.Distance(transform.position, dest) <= waypointReachEps)
        {
            _wpIndex = (_wpIndex + 1) % waypoints.Length;
            if (idleAtPointTime > 0f)
            {
                _idleTimer = idleAtPointTime;
                Stop();
                if (animator) animator.SetBool("IsPatrolling", false);
            }
        }
    }

    // לוגיקת מרדף - זהה לחלוטין ל-EnemyMelee
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
        if (distX > Mathf.Max(stopDistance, 0.01f))
            MoveTowards(chasePos);
        else
            Stop();
    }

    // לוגיקת תקיפה - תנועה זהה ל-EnemyMelee + ירי
    protected override void AttackTick()
    {
        if (!target)
        {
            Stop();
            return;
        }

        // --- חלק התנועה: זהה ל-EnemyMelee ---
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
            if (distX > stopDistance)
                MoveTowards(lockedPos); // אם השחקן זז, נתקרב אליו
            else
                Stop(); // אם אנחנו מספיק קרובים, נעצור
        }

        // --- חלק הירי: הפעולה הייחודית ל-Shooter ---
        cd -= Time.deltaTime;
        if (cd <= 0f && projectilePrefab && muzzle)
        {
            var go = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
            if (go.TryGetComponent<Projectile>(out var proj))
            {
                proj.ownerTag = "Enemy";
                proj.damage = damage;
                proj.speed = projectileSpeed;
                var dir = (target.position - muzzle.position).normalized;
                proj.Launch(dir);
            }
            cd = 1f / Mathf.Max(shotsPerSecond, 0.01f);
        }
    }
}