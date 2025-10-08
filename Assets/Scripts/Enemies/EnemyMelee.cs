using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Melee")]
    [SerializeField, Min(1)] private int meleeDamage = 12;
    [SerializeField, Min(0f)] private float meleeRange = 1.1f;   // טווח פגיעה
    [SerializeField, Min(0f)] private float hitCooldown = 0.8f;  // שניות בין פגיעות
    [SerializeField, Min(0f)] private float stopDistance = 0.6f; // מרחק לעצירה לפני היעד

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

    // --- צ'ייס רק בציר X ---
    protected override void ChaseTick()
    {
        if (!target)
        {
            Stop();
            return;
        }

        // נרדוף רק על ציר X: לוקחים את X של השחקן אבל משאירים את Y הנוכחי של האויב
        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 chasePos = new Vector2(targetPos.x, selfPos.y);

        // מתקרבים עד מרחק עצירה (כדי לא "לרקוד" על הקוליידר של השחקן)
        float distX = Mathf.Abs(targetPos.x - selfPos.x);
        if (distX > Mathf.Max(stopDistance, 0.01f))
            MoveTowards(chasePos);
        else
            Stop();
    }

    // --- גם בהתקפה ננעל על גובה האויב כדי לא לרחף ---
    protected override void AttackTick()
    {
        if (!target)
        {
            Stop();
            return;
        }

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 lockedPos = new Vector2(targetPos.x, selfPos.y); // נועל Y

        // נעמוד קרוב ליעד כדי להבטיח פגיעה, אבל רק על ציר X
        float distX = Mathf.Abs(targetPos.x - selfPos.x);
        if (distX > stopDistance)
            MoveTowards(lockedPos);
        else
            Stop();

        // קצב פגיעה
        _cooldown -= Time.deltaTime;
        if (_cooldown > 0f) return;

        // בדיקת טווח פגיעה: נבדוק מרחק אמיתי אבל בדרך כלל עדיף להסתמך על טריגר/היטבוקס
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= meleeRange && target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(meleeDamage);
            _cooldown = hitCooldown;
        }
    }
}
