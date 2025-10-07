using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private bool isPlayer;

    public int Max => maxHealth;
    public int Current { get; private set; }

    void Awake() => Current = maxHealth;

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Min(Current + amount, maxHealth);
        if (isPlayer) GameManager.I?.NotifyPlayerHealth(Current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || Current <= 0) return;
        Current -= amount;
        if (isPlayer) GameManager.I?.NotifyPlayerHealth(Current, maxHealth);
        if (Current <= 0) Die();
    }

    private void Die()
    {
        if (isPlayer) return; // GameManager כבר מטפל בגיימאובר דרך השחקן
        Destroy(gameObject);
    }
}

public interface IDamageable
{
    void TakeDamage(int amount);
}
