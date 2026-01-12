using UnityEngine;

public class Character : MonoBehaviour
{
    // Startvärde på hälsa, kan justeras i Unity Inspector
    [SerializeField] private int _health = 100;

    public int Health
    {
        get { return _health; }
        set
        {
            _health = Mathf.Clamp(value, 0, 100); // Hindra att det går under 0 eller över 100

            if (_health == 0)
            {
                Die();
            }
        }
    }
    public virtual void takeDamage(int amount)
    {
        Health -= amount;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
