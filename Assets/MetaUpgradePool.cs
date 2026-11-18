using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Meta Upgrade Pool", fileName = "MetaUpgradePool")]
public class MetaUpgradePool : ScriptableObject
{
    public List<MetaUpgradeData> upgrades = new List<MetaUpgradeData>();
}