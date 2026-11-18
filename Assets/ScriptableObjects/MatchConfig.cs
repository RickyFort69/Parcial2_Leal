using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Match Config", fileName = "MatchConfig")]
public class MatchConfig : ScriptableObject
{
    [Min(10)] public float survivalTime = 300f;

    [Header("Chatarra (progresión permanente)")]
    [Range(0f, 1f)] public float defeatKeepPercent = 0.75f;   // al perder te quedás con este porcentaje
    public float victoryMultiplier = 1.5f;                    // al ganar cobrás un bonus

    [Header("Escenas")]
    public string mainMenuScene = "MainMenu";
}