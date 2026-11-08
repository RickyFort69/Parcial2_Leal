using UnityEngine;

public enum StatType { Damage, FireRate, MoveSpeed, TurnSpeed, MaxHealth, PickupRange, ProjectileSpeed, Range }

[CreateAssetMenu(menuName = "Tank/Upgrade", fileName = "NewUpgrade")]
public class UpgradeData : ScriptableObject
{
    public string title;
    [TextArea] public string description;
    public Sprite icon;
    public StatType stat;
    public float amount;
    public bool isPercent;
}