using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Enemy Data", fileName = "NewEnemy")]
public class EnemyData : ScriptableObject
{
    public string displayName;
    public Enemy prefab;                 // prefab con el componente Enemy
    public float maxHealth = 30f;
    public float speed = 3f;
    public float contactDamage = 5f;
    public float damageCooldown = 0.8f;
    public int xpValue = 1;
    public XPGem gemPrefab;              // la gema que suelta
    public int unlockWave = 1;
    public float contactRange = 1.3f; // desde qué oleada aparece
}