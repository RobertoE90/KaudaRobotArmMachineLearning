using UnityEngine;


public class NemaMotorControllerFreeAngleRotator : NemaMotorControllerBase
{
    private float _targetRotationValue = 0f;

    public override void SetTargetValue(float normalizedRotationValue)
    {
        _targetRotationValue = normalizedRotationValue * 360;
    }

    public override void Tick()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            Quaternion.Euler(_targetRotationValue * GetRotationAxis()),
            _rotationSpeed * Time.deltaTime);
    }
}
