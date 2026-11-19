using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public PlayerXP playerXP;
    public PlayerStats stats;
    public List<UpgradeData> pool;
    public GameObject panel;

    [Header("Cartas")]
    public UpgradeCard cardPrefab;      // el PREFAB de la carta (desde Project)
    public Transform cardsContainer;    // padre donde aparecen las cartas (dentro del panel)
    public int optionsCount = 3;

    readonly HashSet<UpgradeData> taken = new HashSet<UpgradeData>();
    readonly List<GameObject> spawned = new List<GameObject>();

    void OnEnable() => playerXP.OnLevelUp += Show;
    void OnDisable() => playerXP.OnLevelUp -= Show;

    void Show(int level)
    {
        var options = RollOptions(optionsCount);
        if (options.Count == 0) return;

        Time.timeScale = 0f;
        panel.SetActive(true);

        foreach (var go in spawned) if (go != null) Destroy(go);
        spawned.Clear();

        foreach (var opt in options)
        {
            var card = Instantiate(cardPrefab, cardsContainer);
            card.gameObject.SetActive(true);
            card.Setup(opt, StatLine(opt), Choose);
            spawned.Add(card.gameObject);
        }
    }

    string StatLine(UpgradeData u)
    {
        float now = stats.GetStat(u.stat);
        float next = stats.GetPreview(u);
        return StatName(u.stat) + ": " + now.ToString("0.##") + " → " + next.ToString("0.##");
    }

    static string StatName(StatType s)
    {
        switch (s)
        {
            case StatType.Damage: return "Daño";
            case StatType.FireRate: return "Cadencia";
            case StatType.MoveSpeed: return "Velocidad";
            case StatType.TurnSpeed: return "Giro";
            case StatType.MaxHealth: return "Vida máxima";
            case StatType.PickupRange: return "Recolección";
            case StatType.ProjectileSpeed: return "Vel. de bala";
            case StatType.Range: return "Alcance";
            case StatType.HealthRegen: return "Regeneración";
            case StatType.ExtraProjectiles: return "Proyectiles";
            case StatType.Pierce: return "Perforación";
            default: return s.ToString();
        }
    }

    List<UpgradeData> RollOptions(int count)
    {
        var candidates = pool.Where(IsAvailable).ToList();
        var result = new List<UpgradeData>();

        while (result.Count < count && candidates.Count > 0)
        {
            float total = candidates.Sum(u => u.weight);
            float roll = Random.value * total;
            UpgradeData chosen = candidates[candidates.Count - 1];
            foreach (var u in candidates)
            {
                roll -= u.weight;
                if (roll <= 0f) { chosen = u; break; }
            }
            result.Add(chosen);
            candidates.Remove(chosen);
        }
        return result;
    }

    bool IsAvailable(UpgradeData u)
    {
        if (u == null) return false;
        if (u.requiresUnlock && !MetaProgress.IsUnlocked(u)) return false;
        if (u.unique && taken.Contains(u)) return false;
        if (u.requires != null && !taken.Contains(u.requires)) return false;
        return true;
    }

    void Choose(UpgradeData u)
    {
        taken.Add(u);
        stats.Apply(u);
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}