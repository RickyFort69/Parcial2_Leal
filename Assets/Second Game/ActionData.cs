using UnityEngine;

public enum ActionKind { Attack, Magic, Item, Limit }
public enum TargetType { SingleEnemy, AllEnemies, SingleAlly, AllAllies }
public enum EffectType { Damage, HealHP, HealMP }

[CreateAssetMenu(menuName = "FF/Action", fileName = "NewAction")]
public class ActionData : ScriptableObject
{
    public string displayName = "Ataque";
    public ActionKind kind = ActionKind.Attack;

    [Tooltip("Siempre desde el punto de vista de quien la usa: 'Enemy' = sus rivales")]
    public TargetType target = TargetType.SingleEnemy;
    public EffectType effect = EffectType.Damage;

    public bool usesMagic;          // true = usa MAG y defensa mágica
    public float power = 1f;        // multiplicador
    public int mpCost;
    public int fixedAmount;         // para objetos: cantidad fija (si es mayor a 0 ignora la fórmula)

    [TextArea] public string description;
}