using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

public class PoolDelegate : MonoBehaviour {

    private IObjectPool<GameObject> _pool;

    public void Attach(IObjectPool<GameObject> pool) {
        Assert.IsTrue(_pool == null, "already attached");
        _pool = pool;
    }

    public void Release() {
        Assert.IsTrue(_pool != null, "not attached");
        _pool.Release(gameObject);
    }
}

public class EnemyGenerator : MonoBehaviour {

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform initialTarget;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int enemiesPerSpawn = 10;
    [SerializeField] private int maxPoolSize = 30;

    private IObjectPool<GameObject> _pool;

    private void Awake() {
        _pool = new LinkedPool<GameObject>(OnCreateEnemy, OnTakeFromPool, OnReleaseToPool, OnDestroyEnemy, true, maxPoolSize);
    }

    private IEnumerator Start() {
        while (true) {
            for (int i = 0; i < enemiesPerSpawn; i++) {
                GenerateNext();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private GameObject OnCreateEnemy() {
        var enemyObject = Instantiate(enemyPrefab, enemyContainer);
        var enemy = enemyObject.AddComponent<MeleEnemyBehaviour>();
        enemy.OnDeath += () => _pool.Release(enemyObject);
        return enemyObject;
    }

    private void OnTakeFromPool(GameObject enemyObject) {
        enemyObject.SetActive(true);
        var health = enemyObject.GetComponent<Health>();
        health.Full();
        var behaviour = enemyObject.GetComponent<MeleEnemyBehaviour>();
        behaviour.SetPathTarget(initialTarget);
        behaviour.Patrol();
    }

    private void OnReleaseToPool(GameObject enemyObject) {
        enemyObject.SetActive(false);
    }

    private void OnDestroyEnemy(GameObject enemyObject) {
        Destroy(enemyObject);
    }

    public void GenerateNext() {
        var enemy = _pool.Get();
        enemy.transform.position = spawnPoint.position;
    }

}
