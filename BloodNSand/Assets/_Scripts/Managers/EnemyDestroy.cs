using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyDestroy : MonoBehaviour
{
    IObjectPool<GameObject> enemyPool;

    public void SetPool(IObjectPool<GameObject> pool)
    {
        enemyPool = pool;
    }

    private void OnTriggerEnter(Collider other)
    {
        enemyPool.Release(gameObject);
    }
}
