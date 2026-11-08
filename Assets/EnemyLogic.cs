using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Health))]
public class Enemy : MonoBehaviour
{
    EnemyData data;
    float speed = 3f;
    float contactDamage = 5f;
    float damageCooldown = 0.8f;

    Transform player;
    Rigidbody rb;
    Health health;
    float nextDamageTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        health.OnDied += Die;
    }

    public void Init(EnemyData d, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        data = d;
        speed = d.speed * speedMultiplier;
        contactDamage = d.contactDamage;
        damageCooldown = d.damageCooldown;
        health.maxHealth = d.maxHealth * healthMultiplier;
        health.current = health.maxHealth;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            Debug.LogError("No hay ningún objeto con el tag Player en la escena", this);
            enabled = false;
            return;
        }
        player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 dir = player.position - rb.position;
        dir.y = 0f;
        dir.Normalize();

        Vector3 vel = dir * speed;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;

        if (dir != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(dir));
    }

    void OnCollisionStay(Collision c)
    {
        if (Time.time < nextDamageTime) return;

        var h = c.collider.GetComponentInParent<Health>();
        if (h != null && h.CompareTag("Player"))
        {
            h.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    void Die()
    {
        if (data != null && data.gemPrefab != null)
        {
            var gem = Instantiate(data.gemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            gem.value = data.xpValue;
        }
        Destroy(gameObject);
    }
}