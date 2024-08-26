using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WaveSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public float timeBetweenSpawns = 5f;

    float timeSinceLastSpawn;

    public GameObject enemyPrefab;
    IObjectPool<GameObject> enemyPool;


    private void Awake()
    {
        enemyPool = new ObjectPool<GameObject>(CreateEnemy, OnGet, OnRelease);
    }

    void OnGet(GameObject enemy)
    {
        enemy.SetActive(true);
        Transform randomSpawnPoints = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = randomSpawnPoints.position;
    } 
    void OnRelease(GameObject enemy)
    {
        enemy.SetActive(false);
    }

    GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.GetComponent<EnemyDestroy>().SetPool(enemyPool);
        return enemy;
    }
    void Update()
    {
        if(Time.time > timeSinceLastSpawn)
        {
            enemyPool.Get();
            timeSinceLastSpawn = Time.time + timeBetweenSpawns;
        }
    }
}
