using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Upgrade Pool", fileName = "UpgradePool")]
public class UpgradePool : ScriptableObject
{
    [Min(1)] public int choicesPerLevel = 3;
    public List<UpgradeData> upgrades = new List<UpgradeData>();

    public List<UpgradeData> GetRandom(int count)
    {
        return upgrades.Where(u => u != null)
                       .OrderBy(_ => Random.value)
                       .Take(count)
                       .ToList();
    }
}