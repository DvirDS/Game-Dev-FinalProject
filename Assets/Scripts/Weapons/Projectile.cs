using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Config")]
    public int damage = 10;
    public float speed = 12f;
    public float lifeTime = 3f;
    public LayerMask hitMask;     
    public string ownerTag = "Player"; 

    [SerializeField] private bool rotateToDirection = true;

    private Vector2 dir;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void Update()
    {
        float distanceToMove = speed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distanceToMove, hitMask);

        if (hit.collider != null)
        {
            if (!hit.collider.CompareTag(ownerTag))
            {
                transform.position = hit.point;
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(damage);
                }
                Destroy(gameObject);
                return;
            }
        }

        transform.Translate(dir * distanceToMove, Space.World);
    }
}