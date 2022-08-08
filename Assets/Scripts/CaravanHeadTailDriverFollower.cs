using System;
using UnityEngine;

[RequireComponent(typeof(CaravanMember))]
public class CaravanHeadTailDriverFollower : MonoBehaviour {
    
    private TwoAxisVehicleFollowDriver _followDriver;

    private void Awake() {
        _followDriver = GetComponent<TwoAxisVehicleFollowDriver>();
        
        var member = GetComponent<CaravanMember>();
        member.OnHeadChanged += UpdateDriverWithNewTarget;
        UpdateDriverWithNewTarget(member);
    }

    private void UpdateDriverWithNewTarget(CaravanMember source) {
        var newHeadConnectionPoint = source.Head == null ? null : source.Head.GetComponent<ConnectionPoint>();
        _followDriver.SetFollowPoint(newHeadConnectionPoint);
    }
}