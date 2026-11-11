using UnityEngine;

[CreateAssetMenu(menuName = "FF/Wave", fileName = "NewWave")]
public class WaveData : ScriptableObject
{
    public FFEnemyData[] enemies;     // un enemigo por posición, máximo 4
}