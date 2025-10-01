using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Melee")]
    [SerializeField, Min(1)] private int meleeDamage = 12;
    [SerializeField, Min(0f)] private float meleeRange = 1.1f;   // טווח פגיעה
    [SerializeField, Min(0f)] private float hitCooldown = 0.8f;   // שניות בין פגיעות
    [SerializeField, Min(0f)] private float stopDistance = 0.6f;   // מרחק לעצירה לפני היעד

    [Header("Patrol (Optional)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0f)] private float waypointReachEps = 0.1f;
    [SerializeField, Min(0f)] private float idleAtPointTime = 0.0f;

    private int _wpIndex;
    private float _cooldown;
    private float _idleTimer;

    protected override void PatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0) { Stop(); return; }

        // אם יש השהייה – עוצרים ונראים Idle (IsPatrolling=false)
        if (_idleTimer > 0f)
        {
            _idleTimer -= Time.deltaTime;
            Stop();

            if (animator) animator.SetBool("IsPatrolling", false);

            // כשנגמר – חוזרים להליכה (IsPatrolling=true)
            if (_idleTimer <= 0f && animator)
                animator.SetBool("IsPatrolling", true);

            return;
        }

        // תנועה ל-WP הנוכחי
        var dest = (Vector2)waypoints[_wpIndex].position;
        MoveTowards(dest);

        // הגענו?
        if (Vector2.Distance(transform.position, dest) <= waypointReachEps)
        {
            _wpIndex = (_wpIndex + 1) % waypoints.Length;

            if (idleAtPointTime > 0f)
            {
                _idleTimer = idleAtPointTime;
                Stop();

                // מיידית נעבור ויזואלית ל-Idle
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

        // מתקרבים עד מרחק עצירה (כדי לא "לרקוד" על הקוליידר של השחקן)
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > Mathf.Max(stopDistance, 0.01f))
            MoveTowards(target.position);
        else
            Stop();
    }

    protected override void AttackTick()
    {
        if (!target)
        {
            Stop();
            return;
        }

        // נעמוד קרוב ליעד כדי להבטיח פגיעה
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > stopDistance) MoveTowards(target.position);
        else Stop();

        // קצב פגיעה
        _cooldown -= Time.deltaTime;
        if (_cooldown > 0f) return;

        if (dist <= meleeRange && target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(meleeDamage);
            _cooldown = hitCooldown;
        }
    }
}
