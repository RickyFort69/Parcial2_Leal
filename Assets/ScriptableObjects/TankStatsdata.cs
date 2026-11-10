using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Tank Stats", fileName = "TankStats")]
public class TankStatsData : ScriptableObject
{
    public float maxHealth = 100f;
    public float moveSpeed = 6f;
    public float rotationSpeed = 120f;
    public float fireRate = 2f;          // disparos por segundo
    public float damage = 10f;
    public float projectileSpeed = 18f;
    public float range = 12f;
    public float pickupRange = 3f;

    [Header("Regeneración")]
    public float healthRegen = 0.5f;     // vida por segundo
    public float regenDelay = 3f;        // segundos sin recibir daño antes de empezar a curar
}