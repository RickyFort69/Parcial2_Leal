using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    float damage;

    public void Init(float dmg, float speed, float lifetime = 3f)
    {
        damage = dmg;
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        var h = other.GetComponentInParent<Health>();
        if (h != null)
        {
            if (h.CompareTag("Player")) return;   // no dañar al jugador
            h.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);                  // paredes y obstáculos
        }
    }
}