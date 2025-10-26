using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "MyGame/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public int scoreValue = 10;

    [Header("Line of Sight")]
    [Tooltip("Layers that block enemy vision (e.g. Walls, Ground)")]
    public LayerMask obstacleMask;
    public float lineOfSightYOffset = 0.5f;

    [Header("Health")]
    [Min(1)] public int maxHealth = 20;
    [Min(0f)] public float hurtDuration = 0.2f;
    [Min(0f)] public float deathDestroyDelay = 1.5f;


}