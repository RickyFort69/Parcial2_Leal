using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Match Config", fileName = "MatchConfig")]
public class MatchConfig : ScriptableObject
{
    [Min(10)] public float survivalTime = 300f;   // segundos que hay que aguantar
}