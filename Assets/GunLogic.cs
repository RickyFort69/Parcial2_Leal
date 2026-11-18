using UnityEngine;
using UnityEngine.InputSystem;

public class TankWeapon : MonoBehaviour
{
    [Header("Referencias")]
    public Transform turret;          // la torreta (tu objeto Gun)
    public Transform firePoint;       // objeto vacío en la punta del cañón
    public Projectile projectilePrefab;

    [Header("Stats")]
    public float fireRate = 2f;           // disparos por segundo (attack speed)
    public float damage = 10f;
    public float projectileSpeed = 18f;
    public float range = 12f;
    public float turretTurnSpeed = 720f;  // grados por segundo
    public int projectileCount = 1;
    public int pierce = 0;
    public float spreadAngle = 12f; // grados entre proyectiles
    float nextFireTime;
    Camera cam;

    void Start() => cam = Camera.main;

    void Update()
    {
        if (Time.timeScale == 0f) return;              // pausa / menú de mejoras
        if (Mouse.current == null || cam == null) return;

        AimAtMouse();

        if (Mouse.current.leftButton.isPressed)
            TryShoot();
    }

    void AimAtMouse()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, new Vector3(0f, firePoint.position.y, 0f));
        if (!ground.Raycast(ray, out float dist)) return;

        Vector3 dir = ray.GetPoint(dist) - turret.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        turret.rotation = Quaternion.RotateTowards(
            turret.rotation, Quaternion.LookRotation(dir), turretTurnSpeed * Time.deltaTime);
    }

    void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + 1f / fireRate;

        Vector3 dir = turret.forward;
        dir.y = 0f;
        dir.Normalize();
        Quaternion baseRot = Quaternion.LookRotation(dir);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = (i - (projectileCount - 1) / 2f) * spreadAngle;
            Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 0f);
            var p = Instantiate(projectilePrefab, firePoint.position, rot);
            p.Init(damage, projectileSpeed, range / projectileSpeed, pierce);
        }
    }
}