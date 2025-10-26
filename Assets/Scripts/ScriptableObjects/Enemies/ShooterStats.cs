using UnityEngine;

[CreateAssetMenu(fileName = "New Shooter Stats", menuName = "Game/Shooter Stats")]
public class ShooterStats : EnemyStats
{
    [Header("Shooting")]
    public Projectile projectilePrefab;
    public float shotsPerSecond = 1.5f;
    public int damage = 8;
    public float projectileSpeed = 10f;
    [Min(0f)] public float stopDistance = 3f;
}