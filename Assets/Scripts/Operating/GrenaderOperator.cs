using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;

public class GrenaderOperator : MonoBehaviour {

    private enum ReadyState {
        WaitingForAmmo,
        Preparing,
        Ready
    }

    [SerializeField] private Ammo ammo;
    [SerializeField] private Grenader grenader;
    [SerializeField] private float reloadTime = 0.3f;

    private ReadyState _state = ReadyState.Ready;
    private Coroutine _loadingCoroutine;

    public bool ReadyForActivation => _state == ReadyState.Ready;
    public float TimeToReadynes => _state == ReadyState.WaitingForAmmo ? float.PositiveInfinity : _state == ReadyState.Ready ? 0 : reloadTime;
    public float ExplosionRadius => grenader.ExplosionRadius;
    public Vector3 LauncherPosition => grenader.LauncherPosition;
    public bool IsActivated => _state == ReadyState.Ready && grenader.IsAimActivated;

    public event Action<float> OnStartReload;
    public event Action OnReloaded;

    private void Awake() {
        ammo.OnAmmoStateChanged += AmmoRefillObserver;
    }

    private void OnDestroy() {
        ammo.OnAmmoStateChanged -= AmmoRefillObserver;
    }

    private void AmmoRefillObserver(Ammo ammo) {
        if (_state == ReadyState.WaitingForAmmo && ammo.HasAmmo) {
            Reload();
        }
    }

    public void Aim(Vector3 point) {
        if (_state == ReadyState.WaitingForAmmo) {
            Assert.IsFalse(ammo.HasAmmo, "Invalid state while ammo is available");
            ammo.NotifyNeedAmmo();
            return;
        }

        if (_state == ReadyState.Preparing) {
            return;
        }

        if (_state == ReadyState.Ready) {
            if (!grenader.IsAimActivated) {
                grenader.ActivateAim(point);
            }

            grenader.ChangeAim(point);
            return;
        }
    }

    public void Cancel() {
        grenader.DeactivateAim();
    }

    public void Fire() {
        if (_state == ReadyState.Ready) {
            if (ammo.HasAmmo) {
                Assert.IsTrue(ammo.TakeAmmo());
                grenader.SingleFire();
            }

            Reload();
        }
    }

    private void Reload() {
        if (_loadingCoroutine != null) {
            StopCoroutine(_loadingCoroutine);
        }

        _state = ReadyState.WaitingForAmmo;
        if (ammo.HasAmmo) {    
            _loadingCoroutine = StartCoroutine(LoadingRoutine());
        } else {
            ammo.NotifyNeedAmmo();
        }
    }

    private IEnumerator LoadingRoutine() {
        _state = ReadyState.Preparing;
        OnStartReload?.Invoke(reloadTime);
        yield return new WaitForSeconds(reloadTime);
        
        _state = ReadyState.Ready;
        OnReloaded?.Invoke();
    }

}