using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    float damage;
    int pierceLeft;

    public void Init(float dmg, float speed, float lifetime, int pierce = 0)
    {
        damage = dmg;
        pierceLeft = pierce;
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Health>(out var h))
        {
            h.TakeDamage(damage);
            if (pierceLeft > 0) pierceLeft--;
            else Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (pierceLeft > 0) pierceLeft--;
            else Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}