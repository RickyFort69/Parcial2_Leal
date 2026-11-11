using UnityEngine;

[CreateAssetMenu(menuName = "FF/Enemy", fileName = "NewEnemy")]
public class FFEnemyData : ScriptableObject
{
    public string displayName = "Behemoth";
    public Sprite sprite;
    public bool flipX;

    [Header("Stats")]
    public int maxHP = 300;
    public int attack = 25;
    public int magic = 15;
    public int defense = 5;
    public int magicDefense = 5;
    public int speed = 25;

    [Header("IA: elige una al azar cada vez que le toca")]
    public ActionData[] actions;
}