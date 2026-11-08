using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public WaveConfig config;

    [Header("Zona de spawn (centrada en este objeto)")]
    public Vector2 arenaSize = new Vector2(30f, 30f);
    public float spawnY = 0.5f;
    public float minDistanceFromPlayer = 8f;

    public int Wave { get; private set; }
    public int Alive { get; private set; }
    public event System.Action<int> OnWaveStarted;

    Transform player;
    readonly List<EnemyData> available = new List<EnemyData>();

    void Start()
    {
        if (config == null || config.enemies == null || config.enemies.Length == 0)
        {
            Debug.LogError("WaveSpawner: falta el WaveConfig o no tiene enemigos", this);
            enabled = false;
            return;
        }

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            Debug.LogError("WaveSpawner: no hay ningún objeto con el tag Player", this);
            enabled = false;
            return;
        }

        player = p.transform;
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(config.firstWaveDelay);

        while (true)
        {
            Wave++;
            OnWaveStarted?.Invoke(Wave);
            RefreshAvailable();

            int count = config.baseCount + (Wave - 1) * config.extraPerWave;
            for (int i = 0; i < count; i++)
            {
                while (Alive >= config.maxAlive) yield return null;
                if (player == null) yield break;

                SpawnOne();
                yield return new WaitForSeconds(config.spawnInterval);
            }

            yield return new WaitForSeconds(config.timeBetweenWaves);
        }
    }

    void RefreshAvailable()
    {
        available.Clear();
        foreach (var e in config.enemies)
            if (e != null && e.unlockWave <= Wave) available.Add(e);

        if (available.Count == 0) available.Add(config.enemies[0]);
    }

    void SpawnOne()
    {
        var data = available[Random.Range(0, available.Count)];
        var enemy = Instantiate(data.prefab, GetSpawnPosition(), Quaternion.identity);

        float hpMul = 1f + (Wave - 1) * config.healthPerWave;
        float spMul = Mathf.Min(config.maxSpeedMultiplier, 1f + (Wave - 1) * config.speedPerWave);
        enemy.Init(data, hpMul, spMul);

        Alive++;
        bool counted = false;
        enemy.GetComponent<Health>().OnDied += () =>
        {
            if (counted) return;
            counted = true;
            Alive--;
        };
    }

    Vector3 GetSpawnPosition()
    {
        float hx = arenaSize.x / 2f;
        float hz = arenaSize.y / 2f;
        Vector3 pos = transform.position;

        for (int i = 0; i < 20; i++)
        {
            float x, z;
            switch (Random.Range(0, 4))
            {
                case 0: x = Random.Range(-hx, hx); z = hz; break;
                case 1: x = Random.Range(-hx, hx); z = -hz; break;
                case 2: x = hx; z = Random.Range(-hz, hz); break;
                default: x = -hx; z = Random.Range(-hz, hz); break;
            }

            pos = new Vector3(transform.position.x + x, spawnY, transform.position.z + z);
            if ((pos - player.position).sqrMagnitude >= minDistanceFromPlayer * minDistanceFromPlayer)
                break;
        }
        return pos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * spawnY,
                            new Vector3(arenaSize.x, 0.1f, arenaSize.y));
    }
}