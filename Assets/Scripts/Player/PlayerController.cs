using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private Animator animator;          // �� ��� � ���� �-Awake
    [SerializeField] private SpriteRenderer sprite;      // �� ��� � ���� �-Awake

    [Header("Move")]
    [SerializeField, Min(0f)] private float baseSpeed = 6f;
    [SerializeField, Min(0f)] private float acceleration = 60f;   // ��� ��� ������ ������� ����
    [SerializeField, Min(0f)] private float deceleration = 80f;   // ����� ����� ���

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpImpulse = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody2D rb;
    private bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        if (!sprite) sprite = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        // �����: Gravity Scale 3�7, Linear Drag = 0 ����������
    }

    void Update()
    {
        grounded = IsGrounded();

        // ����� � ����� �����-������ ��-InputReader ������ �� �����
        if (input.JumpPressed && grounded)
        {
            // ������ ������-����� ����� ���� ����
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

            if (animator) animator.SetTrigger("IsJumping");
        }

        // ��������/����� ������
        if (animator) animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        if (sprite && Mathf.Abs(input.Move.x) > 0.01f)
            sprite.flipX = input.Move.x < 0f;
    }

    void FixedUpdate()
    {
        float speed = baseSpeed * (input.SprintHeld ? 1.5f : 1f);
        float x = input.Move.x;
        float targetVx = Mathf.Abs(x) > 0.01f ? Mathf.Sign(x) * speed : 0f;

        // ��� ������ ������
    

        // ������ �����/����� ����� ������
        float accel = Mathf.Abs(targetVx) > 0.01f ? acceleration : deceleration;

        // ������ X ����
        float newVx = Mathf.MoveTowards(rb.linearVelocity.x, targetVx, accel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
    }

    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
