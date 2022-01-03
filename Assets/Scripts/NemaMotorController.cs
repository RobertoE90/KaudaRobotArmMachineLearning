using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NemaMotorController : MonoBehaviour
{
    [SerializeField] private RotationAxis _rotationAxis;
    [SerializeField] private float _rotationSpeed;
    [Space(20)]
    [SerializeField] private float _minAngle;
    [SerializeField] private float _maxAngle;

    [SerializeField] private bool _debug = false; 
    private float _currentNormalizedRotationValue;
    private float _targetNormalizedRotationValue;
    private float _rotationStep;

    private void Awake()
    {
        _currentNormalizedRotationValue = 0;
        _rotationStep = _rotationSpeed / (_maxAngle - _minAngle);
        SetJointRotation(_currentNormalizedRotationValue);
    }

    public void SetTargetValue(float value)
    {
        _targetNormalizedRotationValue = value;
    }

    public void Tick()
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

    private Vector3 GetRotationAxis()
    {
        var result = Vector3.zero;
        switch (_rotationAxis)
        {
            case RotationAxis.XAxis:
                result = Vector3.right;
                break;
            case RotationAxis.YAxis:
                result = Vector3.up;
                break;
            case RotationAxis.ZAxis:
                result = Vector3.forward;
                break;
        }

        return result;
    }
}

[Serializable]
public enum RotationAxis
{
    XAxis,
    YAxis,
    ZAxis
}
