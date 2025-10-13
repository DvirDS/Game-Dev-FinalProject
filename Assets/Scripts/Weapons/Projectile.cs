using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Config")]
    public int damage = 10;
    public float speed = 12f;
    public float lifeTime = 3f;
    public LayerMask hitMask;     // שכבות בהן הכדור יכול לפגוע
    public string ownerTag = "Player"; // תג של מי שירה את הכדור

    // --- חדש: מתג שקובע אם הקליע צריך להסתובב ---
    [SerializeField] private bool rotateToDirection = true;

    private Vector2 dir;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;

        // --- חדש: בלוק קוד שמסובב את הקליע רק אם האפשרות דלוקה ---
        if (rotateToDirection)
        {
            // מחשב את הזווית מתוך כיוון התנועה
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // מסובב את הספרייט לזווית הנכונה
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