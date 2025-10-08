using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float shotsPerSecond = 1.5f;
    [SerializeField] private int damage = 8;
    [SerializeField] private float projectileSpeed = 10f;
    private float cd;

    protected override void PatrolTick()
    {
        // אפשר לשלב כאן Waypoints (ראה הרחבה בהמשך)
        Stop();
    }

    // NEW: Strafe/Chase על ציר X בלי לרדת מהפלטפורמה
    protected override void ChaseTick()
    {
        if (!target) { Stop(); return; }

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 chasePos = new Vector2(targetPos.x, selfPos.y);

        if (!CanStepTowardsX(chasePos))
        {
            Stop();
            return;
        }

        // שמירת מרחק נוח מהשחקן
        float desiredSpacing = Mathf.Max(attackRange * 0.8f, 1.5f);
        float dx = targetPos.x - selfPos.x;
        float absDx = Mathf.Abs(dx);

        if (absDx > desiredSpacing + 0.2f)
            MoveTowards(chasePos);        // להתקרב
        else if (absDx < desiredSpacing - 0.2f)
            MoveTowards(new Vector2(selfPos.x - Mathf.Sign(dx), selfPos.y)); // להתרחק מעט
        else
            Stop();
    }

    protected override void AttackTick()
    {
        if (!target) { Stop(); return; }
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
        Stop();
    }
}
