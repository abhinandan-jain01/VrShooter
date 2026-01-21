using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Enemy Prefabs")]
    public GameObject dronePrefab;
    public GameObject runnerPrefab;

    [Header("Spawn Offsets (height difference)")]
    public float droneHeightOffset = 1.6f;
    public float runnerHeightOffset = 0.0f;

    [Header("Wave Timing")]
    public float timeBetweenSpawns = 0.35f;
    public float timeBetweenWaves = 2.0f;

    [Header("Waves (simple 3)")]
    public int wave1Drones = 4;
    public int wave1Runners = 0;

    public int wave2Drones = 3;
    public int wave2Runners = 4;

    public int wave3Drones = 4;
    public int wave3Runners = 5;

    int aliveEnemies = 0;

    void OnEnable()
    {
        EnemyBase.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyBase.OnAnyEnemyDied -= OnEnemyDied;
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (GameManager.I && (GameManager.I.IsLost || GameManager.I.IsWon)) yield break;

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(SpawnWave(1, wave1Drones, wave1Runners));
        yield return StartCoroutine(SpawnWave(2, wave2Drones, wave2Runners));
        yield return StartCoroutine(SpawnWave(3, wave3Drones, wave3Runners));

        if (GameManager.I) GameManager.I.Win();
        Debug.Log("WIN! All waves completed.");
    }

    IEnumerator SpawnWave(int waveNumber, int drones, int runners)
    {
        if (GameManager.I && (GameManager.I.IsLost || GameManager.I.IsWon)) yield break;

        if (GameManager.I) GameManager.I.SetWave(waveNumber);

        // Drones
        for (int i = 0; i < drones; i++)
        {
            Spawn(dronePrefab, droneHeightOffset);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // Runners
        for (int i = 0; i < runners; i++)
        {
            Spawn(runnerPrefab, runnerHeightOffset);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // Wait wave clear
        while (aliveEnemies > 0)
            yield return null;

        yield return new WaitForSeconds(timeBetweenWaves);
    }

    void Spawn(GameObject prefab, float heightOffset)
    {
        if (GameManager.I && (GameManager.I.IsLost || GameManager.I.IsWon)) return;

        if (!prefab || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = sp.position + Vector3.up * heightOffset;

        Instantiate(prefab, pos, Quaternion.identity);
        aliveEnemies++;

        if (GameManager.I) GameManager.I.AddAlive(+1);
    }

    void OnEnemyDied(EnemyBase e)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        if (GameManager.I) GameManager.I.AddAlive(-1);
    }
}
