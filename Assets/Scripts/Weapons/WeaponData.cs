using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    public Sprite icon;

    [Header("Stats")]
    public int damage = 10;

    // fireRate = כדורים לשנייה (ה-WeaponController מחשב 1/fireRate)
    public float fireRate = 5f;

    // מהירות הקליע; שומר תאימות לשם הישן "bulletForce"
    [FormerlySerializedAs("bulletForce")]
    public float muzzleVelocity = 14f;

    // מצב ירי; שומר תאימות לשם הישן "autoFire"
    [FormerlySerializedAs("autoFire")]
    public bool isAutomatic = true;

    [Header("Firing Pattern")]
    [Min(1)] public int bulletsPerShot = 1;            // כמה קליעים בכל ירייה
    [Range(0f, 45f)] public float spreadAngle = 0f;    // זווית מניפה/ריסוס במעלות

    [Header("FX")]
    // פריפאב לקליע; שומר תאימות לשם הישן "bulletPrefab"
    [FormerlySerializedAs("bulletPrefab")]
    public GameObject projectilePrefab;

    public Transform muzzleVFX; // אופציונלי

    [Header("Audio")]
    public AudioClip fireSfx;

    public const float MIN_FIRE_RATE = 0.1f;
}
