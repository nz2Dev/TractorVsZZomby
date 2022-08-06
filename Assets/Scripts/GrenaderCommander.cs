using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GrenaderCommander : MonoBehaviour {

    [SerializeField] private GroundObservable groundObservable;
    [SerializeField] private bool singleFireMode;

    private CaravanSelection _grenaders;
    private GrenaderController _singleFireGrenader;

    private Vector3 _aimPoint;

    public void Activate(CaravanSelection greandersSelection) {
        _grenaders = greandersSelection;
        groundObservable.OnEvent += OnGroundEvent;
    }

    public void Deactivate() {
        groundObservable.OnEvent -= OnGroundEvent;
        _grenaders = null;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            singleFireMode = !singleFireMode;
        }

        if (Input.GetKeyDown(KeyCode.R) && _grenaders != null) {
            foreach (var greander in _grenaders.SelectedMembers) {
                var ammo = greander.GetComponent<Ammo>();
                ammo.RefillFull();
            }
        }

        if (_aimPoint != default) {
            if (singleFireMode) {
                AimSingleGreander();
            } else {
                AimAllGreanders();
            }
        }
    }

    private void OnGroundEvent(GroundObservable.EventType eventType, PointerEventData eventData) {
        switch (eventType) {
            case GroundObservable.EventType.PointerDown:
                _aimPoint = eventData.pointerCurrentRaycast.worldPosition;
                if (singleFireMode) {
                    ActivateSingleGreander();
                } else {
                    ActivateAllGreanders();
                }
                break;

            case GroundObservable.EventType.PointerDrag:
                _aimPoint = eventData.pointerCurrentRaycast.worldPosition;
                break;

            case GroundObservable.EventType.PointerUp:
                if (singleFireMode) {
                    FireSingleGreander();
                } else {
                    FireAllGreanders();
                }
                _aimPoint = default;
                break;

            default:
                throw new System.Exception();
        }
    }

    private void ActivateSingleGreander() {
        var nextGreander = _grenaders.SelectedMembers
                .Select((member) => member.GetComponent<GrenaderController>())
                .OrderBy((controller) => controller.TimeToReadynes)
                .FirstOrDefault();

        if (nextGreander != null) {
            if (nextGreander.Activate(_aimPoint)) {
                _singleFireGrenader = nextGreander;
            }
        }
    }

    private void AimSingleGreander() {
        if (_singleFireGrenader != null) {
            _singleFireGrenader.Aim(_aimPoint);
        }
    }

    private void FireSingleGreander() {
        if (_singleFireGrenader != null) {
            _singleFireGrenader.Fire();
            _singleFireGrenader = null;
        }
    }

    private void ActivateAllGreanders() {
        foreach (var greanderMember in _grenaders.SelectedMembers) {
            var greanderController = greanderMember.GetComponent<GrenaderController>();
            greanderController.Activate(_aimPoint);
        }
    }

    private void AimAllGreanders() {
        foreach (var greanderMember in _grenaders.SelectedMembers) {
            var greanderController = greanderMember.GetComponent<GrenaderController>();
            greanderController.Aim(_aimPoint);
        }
    }

    private void FireAllGreanders() {
        foreach (var greanderMember in _grenaders.SelectedMembers) {
            var greanderController = greanderMember.GetComponent<GrenaderController>();
            greanderController.Fire();
        }
    }

}