using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Stats", menuName = "Game/Melee Stats")]
public class MeleeStats : EnemyStats
{
    [Header("Melee")]
    [Min(1)] public int meleeDamage = 12;
    [Min(0f)] public float meleeRange = 1.1f;
    [Min(0f)] public float hitCooldown = 0.8f;
    [Min(0f)] public float stopDistance = 0.6f;
}