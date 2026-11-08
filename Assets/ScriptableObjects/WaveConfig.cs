using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Wave Config", fileName = "WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public EnemyData[] enemies;

    [Header("Ritmo")]
    public float firstWaveDelay = 2f;
    public float timeBetweenWaves = 8f;
    public int baseCount = 5;
    public int extraPerWave = 3;
    public float spawnInterval = 0.25f;
    public int maxAlive = 100;

    [Header("Escalado por oleada")]
    public float healthPerWave = 0.10f;      // +10% de vida
    public float speedPerWave = 0.03f;       // +3% de velocidad
    public float maxSpeedMultiplier = 2f;
}