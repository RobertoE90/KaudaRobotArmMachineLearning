using UnityEngine;
using System;


/// <summary>
/// Used for rotations that will not use the closest rotation to go from initial rotation to the target rotation
/// </summary>
public class NemaMotorControllerClampedAngleRotator : NemaMotorControllerBase
{
    
    [Space(20)]
    [SerializeField] private float _minAngle;
    [SerializeField] private float _maxAngle;

    private float _currentNormalizedRotationValue;
    private float _targetNormalizedRotationValue;
    private float _rotationStep;

    private void Awake()
    {
        _currentNormalizedRotationValue = 0;
        _rotationStep = _rotationSpeed / (_maxAngle - _minAngle);
        SetJointRotation(_currentNormalizedRotationValue);
    }

    public override void SetTargetValue(float value)
    {
        _targetNormalizedRotationValue = value;
    }

    public override void Tick()
    {
        var incrementSign = (_currentNormalizedRotationValue < _targetNormalizedRotationValue) ? 1 : -1;

        if(Mathf.Abs(_currentNormalizedRotationValue - _targetNormalizedRotationValue) > _rotationStep * Time.deltaTime)
        {
            _currentNormalizedRotationValue += (incrementSign) * _rotationStep * Time.deltaTime;
        }
        else
        {
            _currentNormalizedRotationValue = _targetNormalizedRotationValue;
        }

        SetJointRotation(_currentNormalizedRotationValue);
    }

    private void SetJointRotation(float normalizedRotation)
    {
        var rotationValue = Mathf.Lerp(_minAngle, _maxAngle, normalizedRotation) * GetRotationAxis();
        transform.localRotation = Quaternion.Euler(rotationValue);
    }
}

[Serializable]
public enum RotationAxis
{
    XAxis,
    YAxis,
    ZAxis
}
