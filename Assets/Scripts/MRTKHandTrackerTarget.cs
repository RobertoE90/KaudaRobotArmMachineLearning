using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;

public class MRTKHandTrackerTarget : MonoBehaviour, IMixedRealityHandJointHandler
{
    //[SerializeField] private Transform _targetTransform;
    [SerializeField] private KaudaArmController _armController;
    [SerializeField] private Interactable _checkButtonInteractable;

    private Handedness _controlHandness;

    private bool _isFollowing = false;

    private void Awake()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);

        _checkButtonInteractable.OnClick.AddListener(ToogleController);

        _controlHandness = Handedness.None;
        _isFollowing = _checkButtonInteractable.IsToggled;
    }

    public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
    {
        MixedRealityPose palmPose;
        if (!eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out palmPose))
            return;

        if (!_isFollowing)
            return;

        if (_controlHandness == Handedness.None)
            _controlHandness = eventData.Handedness;


        if (_controlHandness == Handedness.None)
            return;
        
        /*
        if (eventData.Handedness == _controlHandness)
        {
            _targetTransform.position = palmPose.Position + palmPose.Rotation * Vector3.down * 0.075f;
            _targetTransform.rotation = palmPose.Rotation * Quaternion.Euler(90, 180, 180);
        }
        */

        if (!eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out var indexPose) || !eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out var thumbPose))
            return;

        var openDistance = (indexPose.Position - thumbPose.Position).magnitude;
        openDistance = Mathf.Clamp(openDistance, 0f, 0.15f);
        _armController.OpenCloseRange = 1f - openDistance / 0.15f;
    }

    public void ToogleController()
    {
        _isFollowing = _checkButtonInteractable.IsToggled;
        _controlHandness = Handedness.None;
    }
}