using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public TankStatsData baseStats;

    TankController tank;
    TankWeapon weapon;
    PlayerXP xp;
    Health health;

    void Awake()
    {
        tank = GetComponent<TankController>();
        weapon = GetComponent<TankWeapon>();
        xp = GetComponent<PlayerXP>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        if (baseStats == null)
        {
            Debug.LogWarning("PlayerStats: falta asignar TankStatsData", this);
            return;
        }

        tank.moveSpeed = baseStats.moveSpeed;
        tank.rotationSpeed = baseStats.rotationSpeed;
        weapon.fireRate = baseStats.fireRate;
        weapon.damage = baseStats.damage;
        weapon.projectileSpeed = baseStats.projectileSpeed;
        weapon.range = baseStats.range;
        xp.pickupRange = baseStats.pickupRange;
        health.maxHealth = baseStats.maxHealth;
        health.current = baseStats.maxHealth;
    }

    public void Apply(UpgradeData u)
    {
        switch (u.stat)
        {
            case StatType.Damage: weapon.damage = Mod(weapon.damage, u); break;
            case StatType.FireRate: weapon.fireRate = Mod(weapon.fireRate, u); break;
            case StatType.MoveSpeed: tank.moveSpeed = Mod(tank.moveSpeed, u); break;
            case StatType.TurnSpeed: tank.rotationSpeed = Mod(tank.rotationSpeed, u); break;
            case StatType.PickupRange: xp.pickupRange = Mod(xp.pickupRange, u); break;
            case StatType.ProjectileSpeed: weapon.projectileSpeed = Mod(weapon.projectileSpeed, u); break;
            case StatType.Range: weapon.range = Mod(weapon.range, u); break;
            case StatType.MaxHealth:
                health.IncreaseMax(u.isPercent ? health.maxHealth * u.amount / 100f : u.amount);
                break;
        }
    }

    float Mod(float v, UpgradeData u) => u.isPercent ? v * (1f + u.amount / 100f) : v + u.amount;
}