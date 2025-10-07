using UnityEngine;

public class PickupHeal : MonoBehaviour
{
    [SerializeField, Min(1)] private int healAmount = 25;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
