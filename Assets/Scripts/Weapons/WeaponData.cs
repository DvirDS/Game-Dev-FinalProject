using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    public Sprite icon;

    [Header("Stats")]
    public int damage = 10;
    public float fireRate = 5f;         // כדורים לשנייה
    public float muzzleVelocity = 14f;
    public bool isAutomatic = true;

    [Header("FX")]
    public GameObject projectilePrefab;
    public Transform muzzleVFX; // אופציונלי (לא חובה)

    [Header("Audio")]
    public AudioClip fireSfx;

    public const float MIN_FIRE_RATE = 0.1f;
}
