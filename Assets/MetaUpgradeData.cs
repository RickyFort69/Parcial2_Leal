using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Meta Upgrade", fileName = "NewMetaUpgrade")]
public class MetaUpgradeData : ScriptableObject
{
    public string id = "vida";                   // único en cada mejora; no lo cambies después de usarlo
    public string title = "Blindaje";
    [TextArea] public string description;        // si lo dejás vacío se genera solo
    public StatType stat = StatType.MaxHealth;
    public float amountPerLevel = 10f;
    public bool isPercent = true;
    [Min(1)] public int maxLevel = 5;
    [Min(1)] public int baseCost = 20;
    [Min(0)] public int costIncrease = 15;       // cada nivel cuesta esto más

    public int CostForLevel(int currentLevel) => baseCost + costIncrease * currentLevel;
}