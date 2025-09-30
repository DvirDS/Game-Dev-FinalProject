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
