using UnityEngine;

public enum StatType
{
    Damage, FireRate, MoveSpeed, TurnSpeed, MaxHealth, PickupRange, ProjectileSpeed, Range,
    HealthRegen,
    ExtraProjectiles, Pierce
}

[CreateAssetMenu(menuName = "Tank/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string title;
    [TextArea] public string description;
    public Sprite icon;
    public StatType stat;
    public float amount;
    public bool isPercent;

    [Header("Probabilidad")]
    public float weight = 10f;      // más alto = sale más seguido
    public bool unique = false;     // si está marcado, solo se puede elegir una vez
    public UpgradeData requires;    // opcional: solo aparece si ya elegiste esta
}