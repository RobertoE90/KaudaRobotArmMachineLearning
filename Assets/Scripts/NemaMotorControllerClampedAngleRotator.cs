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
        _rotationStep = _rotationSpeed / (_maxAngle - _minAngle);
    }

    public override void InitRotationAt(float normalizedRotation)
    {
        _currentNormalizedRotationValue = normalizedRotation;
        SetJointRotation(_currentNormalizedRotationValue);
    }

    public override void DiscreteControl(int direction)
    {
        if (direction == 0)
        {
            _targetNormalizedRotationValue = _currentNormalizedRotationValue;
            return;
        }

        _targetNormalizedRotationValue = _currentNormalizedRotationValue +
                                         (_rotationStep * Time.deltaTime * 3 * Math.Sign(direction));
        
        _targetNormalizedRotationValue = Mathf.Clamp01(_targetNormalizedRotationValue);
    }

    public override void SetTargetValue(float value)
    {
        _targetNormalizedRotationValue = value;
    }

    public override float Tick()
    {
        var incrementSign = (_currentNormalizedRotationValue < _targetNormalizedRotationValue) ? 1 : -1;

        var tickValue = 0f;

        if(Mathf.Abs(_currentNormalizedRotationValue - _targetNormalizedRotationValue) > _rotationStep * Time.deltaTime)
        {
            tickValue = _rotationStep * Time.deltaTime;
            _currentNormalizedRotationValue += (incrementSign) * tickValue;
        }
        else
        {
            tickValue = Mathf.Abs(_currentNormalizedRotationValue - _targetNormalizedRotationValue);
            _currentNormalizedRotationValue = _targetNormalizedRotationValue;
        }

        SetJointRotation(_currentNormalizedRotationValue);
        return tickValue * (_maxAngle - _minAngle);
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
    XAxis = 0,
    YAxis,
    ZAxis
}
