using UnityEngine;

[CreateAssetMenu(menuName = "FF/Character", fileName = "NewCharacter")]
public class CharacterData : ScriptableObject
{
    public string displayName = "Terra";
    public Sprite sprite;
    public bool flipX;              // marcalo si el dibujo mira para el lado equivocado

    [Header("Stats")]
    public int maxHP = 600;
    public int maxMP = 60;
    public int attack = 30;
    public int magic = 30;
    public int defense = 10;
    public int magicDefense = 10;
    public int speed = 40;          // 40 = unos 2.5 segundos para llenar el ATB

    [Header("Acciones")]
    public ActionData basicAttack;
    public ActionData[] magics;
    public ActionData limitBreak;
}