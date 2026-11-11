using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float current;
    public float invulnerabilityTime = 0f;   // segundos de inmunidad tras recibir daño

    public event Action<float, float> OnChanged; // actual, máxima
    public event Action OnDied;

    float invulnerableUntil;

    void Awake() => current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (current <= 0f) return;                    // ya está muerto
        if (Time.time < invulnerableUntil) return;    // inmune

        invulnerableUntil = Time.time + invulnerabilityTime;
        current = Mathf.Max(0f, current - amount);
        OnChanged?.Invoke(current, maxHealth);
        if (current <= 0f) OnDied?.Invoke();
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