using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    public int price = 100;
    public int ammoPerPurchase = 30;
    public Sprite icon;

    [Header("Stats")]
    public int damage = 10;
    public float fireRate = 5f;

    [FormerlySerializedAs("bulletForce")]
    public float muzzleVelocity = 14f;

    [FormerlySerializedAs("autoFire")]
    public bool isAutomatic = true;

    [Header("Firing Pattern")]
    [Min(1)] public int bulletsPerShot = 1;        
    [Range(0f, 45f)] public float spreadAngle = 0f;    

    [Header("FX")]
    [FormerlySerializedAs("bulletPrefab")]
    public GameObject projectilePrefab;

    public Transform muzzleVFX;

    [Header("Audio")]
    public AudioClip fireSfx;

    public const float MIN_FIRE_RATE = 0.1f;
}
