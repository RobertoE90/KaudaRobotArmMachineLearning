using System;
using UnityEngine;


public class NemaMotorControllerFreeAngleRotator : NemaMotorControllerBase
{
    private float _targetRotationValue = 0f;

    public override void InitRotationAt(float normalizedRotation)
    {
        transform.localRotation = Quaternion.Euler(normalizedRotation * 360 * GetRotationAxis());
    }

    public override void DiscreteControl(int direction)
    {
        var currentRotation = GetRotationValue();
        if (direction == 0)
        {
            _targetRotationValue = currentRotation;
            return;
        }

        _targetRotationValue = currentRotation + (Mathf.Sign(direction) * _rotationSpeed * Time.deltaTime * 3);
        _targetRotationValue %= 360f;
    }

    public override void SetTargetValue(float normalizedRotationValue)
    {
        _targetRotationValue = normalizedRotationValue * 360;
    }

    public override float Tick()
    {
        var targetRotation = Quaternion.Euler(_targetRotationValue * GetRotationAxis());
        var tickValue = Quaternion.Angle(transform.localRotation, targetRotation);

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime);

        return Mathf.Clamp(tickValue, 0, _rotationSpeed * Time.deltaTime);
    }
}
