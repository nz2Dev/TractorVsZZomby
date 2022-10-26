using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TurelOperator : MonoBehaviour {

    [SerializeField] private Turel turel;
    [SerializeField] private float targetSearchInterval = 0.25f;

    private void Start() {
        StartCoroutine(SearchRoutine());
    }

    private IEnumerator SearchRoutine() {
        while (true) {
            var nextTarget = FindClosestEnemy();
            ChangeTarget(nextTarget);
            yield return new WaitForSeconds(targetSearchInterval);
        }
    }

    private EnemyMarker FindClosestEnemy() {
        var enemies = FindObjectsOfType<EnemyMarker>();
        if (enemies.Length <= 0) {
            return null;
        }

        var position = transform.position;
        var closestEnemy = (EnemyMarker) null;
        var closestDistance = float.PositiveInfinity;

        foreach (var enemy in enemies) {
            var enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsZero) {
                var distanceToEnemy = Vector3.Distance(position, enemy.transform.position);
                if (distanceToEnemy < turel.FireRange && distanceToEnemy < closestDistance) {
                    closestDistance = distanceToEnemy;
                    closestEnemy = enemy;
                }    
            }
        }
        
        return closestEnemy;
    }

    private void ChangeTarget(EnemyMarker enemy) {
        if (enemy != null) {
            turel.StartFire(enemy.gameObject);
        } else {
            turel.StopFire();
        }
    }

}