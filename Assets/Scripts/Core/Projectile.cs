using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Config")]
    public int damage = 10;
    public float speed = 12f;
    public float lifeTime = 3f;
    public LayerMask hitMask;     // ????? ????? ??????? ???
    public string ownerTag = "Player"; // ??? ?? ????? ????? ????

    private Vector2 dir;

    void Start() => Destroy(gameObject, lifeTime);

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(ownerTag)) return;
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage);

        Destroy(gameObject);
    }
}