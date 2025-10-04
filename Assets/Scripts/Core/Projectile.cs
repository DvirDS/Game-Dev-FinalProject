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

    private Vector2 dir;

    void Start()
    {
        // עדיין חשוב להשמיד את הכדור אחרי זמן מסוים כדי שלא יישאר לנצח
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;
    }

    void Update()
    {
        // 1. חשב את המרחק שהכדור אמור לעבור בפריים הנוכחי
        float distanceToMove = speed * Time.deltaTime;

        // 2. בצע Raycast מהמיקום הנוכחי אל המיקום הבא
        // ה-Raycast בודק למרחק התנועה המדויק של הפריים הזה
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distanceToMove, hitMask);

        // 3. בדוק אם ה-Raycast פגע במשהו
        if (hit.collider != null)
        {
            // אם הפגיעה היא לא במי שירה את הכדור
            if (!hit.collider.CompareTag(ownerTag))
            {
                // הזז את הכדור בדיוק לנקודת הפגיעה
                transform.position = hit.point;

                // בצע את לוגיקת הפגיעה (מה שהיה קודם ב-OnTriggerEnter2D)
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(damage);
                }

                // השמד את הכדור
                Destroy(gameObject);
                return; // סיים את הפונקציה כדי שהכדור לא יזוז יותר
            }
        }

        // 4. אם ה-Raycast לא פגע בכלום, הזז את הכדור כרגיל
        transform.Translate(dir * distanceToMove, Space.World);
    }
}