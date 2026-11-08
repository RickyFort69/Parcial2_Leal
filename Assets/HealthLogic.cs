using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float current;
    public event Action<float, float> OnChanged; // actual, máxima
    public event Action OnDied;

    void Awake() => current = maxHealth;

    public void TakeDamage(float amount)
    {
        current = Mathf.Max(0, current - amount);
        OnChanged?.Invoke(current, maxHealth);
        if (current <= 0) OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        current = Mathf.Min(maxHealth, current + amount);
        OnChanged?.Invoke(current, maxHealth);
    }

    public void IncreaseMax(float amount)
    {
        maxHealth += amount;
        Heal(amount);
    }
}