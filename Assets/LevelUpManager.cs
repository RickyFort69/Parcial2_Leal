using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public PlayerXP playerXP;
    public PlayerStats stats;
    public List<UpgradeData> pool;
    public GameObject panel;
    public UpgradeCard[] cards;

    readonly HashSet<UpgradeData> taken = new HashSet<UpgradeData>();

    void OnEnable() => playerXP.OnLevelUp += Show;
    void OnDisable() => playerXP.OnLevelUp -= Show;

    void Show(int level)
    {
        var options = RollOptions(cards.Length);
        if (options.Count == 0) return; // nada para ofrecer, no pausamos

        Time.timeScale = 0f;
        panel.SetActive(true);
        for (int i = 0; i < cards.Length; i++)
        {
            bool has = i < options.Count;
            cards[i].gameObject.SetActive(has);
            if (has) cards[i].Setup(options[i], options[i].description, Choose);
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