using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CrowdVehicleDriver))]
[RequireComponent(typeof(CylinderZombie))]
public class MeleEnemyBehaviour : MonoBehaviour {

    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float resumeDistance = 0.3f;
    [SerializeField] private float searchDistance = 20f;
    [SerializeField] private Transform initialPathTarget;

    private bool _chasing;
    private Transform _pathTarget;
    private CrowdVehicleDriver _vehicleDriver;
    private CaravanObserver _caravanObserver;
    private CylinderZombie _zombie;

    private void Awake() {
        _zombie = GetComponent<CylinderZombie>();
        _vehicleDriver = GetComponent<CrowdVehicleDriver>();

        var health = GetComponent<Health>();
        if (health != null) {
            health.OnHealthChanged += comp => {
                if (comp.IsZero) {
                    StopCoroutine(nameof(SearchTarget));
                    _vehicleDriver.SetTarget(null);
                }
            };
        }

        if (initialPathTarget != null) {
            SetPathTarget(initialPathTarget);
        }
    }

    private void Update() {
        if (_vehicleDriver.Target == null) {
            return;
        }

        var vehiclePosition = _vehicleDriver.transform.position;
        var targetPosition = _vehicleDriver.Target.transform.position;
        var distance = Vector3.Distance(targetPosition, vehiclePosition);
        var targetClose = distance < stopDistance;
        var targetFar = distance > resumeDistance;

        if (_chasing && targetClose) {
            _chasing = false;
            _vehicleDriver.Stop();
            _zombie.StartAttack(_vehicleDriver.Target);
        }

        if (!_chasing && targetFar) {
            _chasing = true;
            _vehicleDriver.Resume();
            _zombie.StopAttack();
        }
    }

    private void Start() {
        _caravanObserver = FindObjectOfType<CaravanObserver>();
        StartCoroutine(nameof(SearchTarget));
    }

    public void SetPathTarget(Transform pathTarget) {
        _pathTarget = pathTarget;
    }

    private IEnumerator SearchTarget() {
        while (true) {
            yield return new WaitForSeconds(0.5f);

            var position = transform.position;
            var shortest = (CaravanMember)null;
            var shortestDistance = float.PositiveInfinity;

            if (_caravanObserver != null) {
                foreach (var member in _caravanObserver.CountedMembers) {
                    var distance = Vector3.Distance(position, member.transform.position);
                    if (distance < searchDistance && distance < shortestDistance) {
                        shortestDistance = distance;
                        shortest = member;
                    }
                }
            }

            if (shortest != null) {
                _vehicleDriver.SetTarget(shortest.gameObject);
            } else if (_pathTarget != null) {
                _vehicleDriver.SetTarget(_pathTarget.gameObject);
            }
        }
    }

}