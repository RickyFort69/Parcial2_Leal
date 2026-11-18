using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public TankStatsData baseStats;

    TankController tank;
    TankWeapon weapon;
    PlayerXP xp;
    Health health;
    PlayerRegen regen;
    public MetaUpgradePool metaPool;

    void Awake()
    {
        tank = GetComponent<TankController>();
        weapon = GetComponent<TankWeapon>();
        xp = GetComponent<PlayerXP>();
        health = GetComponent<Health>();
        regen = GetComponent<PlayerRegen>();
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = playerLayer;
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
        health.invulnerabilityTime = baseStats.invulnerabilityTime;

        if (regen != null)
        {
            regen.regenPerSecond = baseStats.healthRegen;
            regen.delayAfterDamage = baseStats.regenDelay;
        }
        ApplyMetaUpgrades();

    }

    public void Apply(UpgradeData u) => ApplyStat(u.stat, u.amount, u.isPercent);

    public void ApplyStat(StatType stat, float amount, bool isPercent)
    {
        float Boost(float v) => isPercent ? v * (1f + amount / 100f) : v + amount;

        switch (stat)
        {
            case StatType.Damage: weapon.damage = Boost(weapon.damage); break;
            case StatType.FireRate: weapon.fireRate = Boost(weapon.fireRate); break;
            case StatType.MoveSpeed: tank.moveSpeed = Boost(tank.moveSpeed); break;
            case StatType.TurnSpeed: tank.rotationSpeed = Boost(tank.rotationSpeed); break;
            case StatType.PickupRange: xp.pickupRange = Boost(xp.pickupRange); break;
            case StatType.ProjectileSpeed: weapon.projectileSpeed = Boost(weapon.projectileSpeed); break;
            case StatType.Range: weapon.range = Boost(weapon.range); break;
            case StatType.MaxHealth:
                health.IncreaseMax(isPercent ? health.maxHealth * amount / 100f : amount);
                break;
            case StatType.HealthRegen:
                if (regen != null) regen.regenPerSecond = Boost(regen.regenPerSecond);
                break;
        }
    }

    public float GetStat(StatType s)
    {
        switch (s)
        {
            case StatType.Damage: return weapon.damage;
            case StatType.FireRate: return weapon.fireRate;
            case StatType.MoveSpeed: return tank.moveSpeed;
            case StatType.TurnSpeed: return tank.rotationSpeed;
            case StatType.PickupRange: return xp.pickupRange;
            case StatType.ProjectileSpeed: return weapon.projectileSpeed;
            case StatType.Range: return weapon.range;
            case StatType.MaxHealth: return health.maxHealth;
            case StatType.HealthRegen: return regen != null ? regen.regenPerSecond : 0f;
            default: return 0f;
        }
    }
    void ApplyMetaUpgrades()
    {
        if (metaPool == null) return;

        foreach (var m in metaPool.upgrades)
        {
            if (m == null) continue;
            int level = MetaProgress.GetLevel(m);
            if (level > 0) ApplyStat(m.stat, m.amountPerLevel * level, m.isPercent);
        }
    }

    public float GetPreview(UpgradeData u)
    {
        float now = GetStat(u.stat);
        return u.isPercent ? now * (1f + u.amount / 100f) : now + u.amount;
    }

    float Mod(float v, UpgradeData u) => u.isPercent ? v * (1f + u.amount / 100f) : v + u.amount;
}