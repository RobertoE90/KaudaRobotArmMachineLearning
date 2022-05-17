using System;
using UnityEngine;

public abstract class NemaMotorControllerBase : MonoBehaviour
{
    [SerializeField] protected RotationAxis _rotationAxis;
    [SerializeField] protected float _rotationSpeed;

    /// <summary>
    /// makes the rotation of the motors with a discrete input
    /// </summary>
    /// <param name="direction">values from -1 to 1 to codify rotation direction or no rotation</param>
    public abstract void DiscreteControl(int direction);
    
    public abstract void SetTargetValue(float normalizedRotationValue);
    public abstract float Tick();

    public abstract void InitRotationAt(float normalizedRotation);

    public float GetRotationValue()
    {
        var angle = Vector3.Scale(transform.localEulerAngles, GetRotationAxis());
        return angle.magnitude;
    }


    /// <summary>
    /// Returns the dot value to avoid rotation jump error
    /// </summary>
    /// <returns></returns>
    public float GetRotationDotValue()
    {
        var perpendicularEnumIndex = (int) (_rotationAxis) + 1;
        perpendicularEnumIndex %= Enum.GetNames(typeof(RotationAxis)).Length;
        var perpendicular = GetRotationAxis((RotationAxis) perpendicularEnumIndex);

        return Vector3.Dot(
            perpendicular,
            transform.localRotation * perpendicular);
    }

    protected Vector3 GetRotationAxis(RotationAxis rAxis)
    {
        var result = Vector3.zero;
        switch (rAxis)
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
    
    protected Vector3 GetRotationAxis()
    {
        return GetRotationAxis(_rotationAxis);
    }
}
