using UnityEngine;

[CreateAssetMenu(menuName = "FF/Battle Config", fileName = "BattleConfig")]
public class BattleConfig : ScriptableObject
{
    [Header("ATB")]
    public float atbSpeedMultiplier = 1f;
    public bool waitWhileMenuOpen = false;     // true = modo "Wait": el tiempo se frena mientras elegís

    [Header("Daño")]
    public float statMultiplier = 2f;
    [Range(0f, 0.5f)] public float variance = 0.1f;
    [Range(0f, 1f)] public float defendReduction = 0.5f;

    [Header("Límite")]
    public float limitGainFactor = 1.5f;       // 1.5 = perder el 20% de vida carga el 30% de la barra

    [Header("Oleadas")]
    public WaveData[] waves;
    public int recoverEveryWaves = 3;
    [Range(0f, 1f)] public float recoverHPPercent = 0.3f;
    [Range(0f, 1f)] public float recoverMPPercent = 0.3f;
}