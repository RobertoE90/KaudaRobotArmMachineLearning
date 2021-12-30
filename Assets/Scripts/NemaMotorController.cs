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
    private float _initialRotationAxisValue;
    private float _targetValue;
    

    public void SetTargetValue(float value)
    {
        var currentLocalRotationEuler = transform.localRotation.eulerAngles;
        var inAxisVector = Vector3.Scale(currentLocalRotationEuler, GetRotationAxis());
        _initialRotationAxisValue = inAxisVector.magnitude;

        _targetValue = Mathf.Lerp(_minAngle, _maxAngle, value);
        _targetValue = _targetValue % 360f;
        _targetValue = Mathf.Clamp(_targetValue, _minAngle, _maxAngle);
    }

    public void Tick()
    {
        var lerpedRotationValue = Mathf.Lerp(_initialRotationAxisValue, _targetValue, _rotationSpeed * Time.deltaTime * 0.1f);

        if(_debug)
            Debug.Log($"init {_initialRotationAxisValue} target{_targetValue} lerped{lerpedRotationValue}");

        transform.localRotation = Quaternion.Euler(lerpedRotationValue * GetRotationAxis());
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
