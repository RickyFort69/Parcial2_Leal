using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Health))]
public class Enemy : MonoBehaviour
{
    EnemyData data;
    float speed = 3f;
    float contactDamage = 5f;
    float damageCooldown = 1f;
    float contactRange = 1.3f;

    Transform player;
    Health playerHealth;
    Rigidbody rb;
    Health health;
    float nextDamageTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        health.OnDied += Die;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = enemyLayer;
    }

    public void Init(EnemyData d, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        data = d;
        speed = d.speed * speedMultiplier * Random.Range(0.9f, 1.1f); // que no vayan todos al mismo paso
        contactDamage = d.contactDamage;
        damageCooldown = d.damageCooldown;
        contactRange = d.contactRange;
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
        playerHealth = p.GetComponent<Health>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - rb.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        Vector3 dir = dist > 0.001f ? toPlayer / dist : Vector3.zero;

        // se frena cuando ya está encima, para no vibrar atravesándote
        Vector3 vel = dist > contactRange * 0.5f ? dir * speed : Vector3.zero;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;

        if (dir != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(dir));

        // daño por contacto, por distancia
        if (dist <= contactRange && playerHealth != null && Time.time >= nextDamageTime)
        {
            playerHealth.TakeDamage(contactDamage);
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
        else
        {
            Debug.LogWarning("Este enemigo no suelta XP: falta el Gem Prefab en su EnemyData", this);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactRange);
    }
}