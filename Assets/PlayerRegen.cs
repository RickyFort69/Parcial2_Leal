using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerRegen : MonoBehaviour
{
    public float regenPerSecond = 0.5f;    // PlayerStats lo pisa con el SO
    public float delayAfterDamage = 3f;

    Health health;
    float lastHealth;
    float lastDamageTime = float.NegativeInfinity;

    void Awake() => health = GetComponent<Health>();
    void Start() => lastHealth = health.current;

    void Update()
    {
        if (health.current <= 0f) return;

        // si la vida bajó desde el frame anterior, recibió daño
        if (health.current < lastHealth) lastDamageTime = Time.time;
        lastHealth = health.current;

        if (regenPerSecond <= 0f) return;
        if (Time.time - lastDamageTime < delayAfterDamage) return;
        if (health.current >= health.maxHealth) return;

        health.Heal(regenPerSecond * Time.deltaTime);
        lastHealth = health.current;
    }
}