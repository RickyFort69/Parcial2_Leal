using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public float contactDamage = 5f;
    public float damageCooldown = 0.8f;
    public XPGem gemPrefab;
    public int xpValue = 1;

    Transform player;
    Rigidbody rb;
    float nextDamageTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Health>().OnDied += Die;
    }

    void Start() => player = GameObject.FindGameObjectWithTag("Player").transform;

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
        if (c.collider.CompareTag("Player") && c.collider.TryGetComponent<Health>(out var h))
        {
            h.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    void Die()
    {
        var gem = Instantiate(gemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        gem.value = xpValue;
        Destroy(gameObject);
    }
}