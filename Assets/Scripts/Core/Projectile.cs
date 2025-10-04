using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Config")]
    public int damage = 10;
    public float speed = 12f;
    public float lifeTime = 3f;
    public LayerMask hitMask;     // ����� ��� ����� ���� �����
    public string ownerTag = "Player"; // �� �� �� ���� �� �����

    private Vector2 dir;

    void Start()
    {
        // ����� ���� ������ �� ����� ���� ��� ����� ��� ��� ����� ����
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;
    }

    void Update()
    {
        // 1. ��� �� ����� ������ ���� ����� ������ ������
        float distanceToMove = speed * Time.deltaTime;

        // 2. ��� Raycast ������� ������ �� ������ ���
        // �-Raycast ���� ����� ������ ������ �� ������ ���
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distanceToMove, hitMask);

        // 3. ���� �� �-Raycast ��� �����
        if (hit.collider != null)
        {
            // �� ������ ��� �� ��� ���� �� �����
            if (!hit.collider.CompareTag(ownerTag))
            {
                // ��� �� ����� ����� ������ ������
                transform.position = hit.point;

                // ��� �� ������ ������ (�� ���� ���� �-OnTriggerEnter2D)
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(damage);
                }

                // ���� �� �����
                Destroy(gameObject);
                return; // ���� �� �������� ��� ������ �� ���� ����
            }
        }

        // 4. �� �-Raycast �� ��� �����, ��� �� ����� �����
        transform.Translate(dir * distanceToMove, Space.World);
    }
}