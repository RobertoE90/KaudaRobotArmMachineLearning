using UnityEngine;

public abstract class NemaMotorControllerBase : MonoBehaviour
{
    [SerializeField] protected RotationAxis _rotationAxis;
    [SerializeField] protected float _rotationSpeed;

    public abstract void SetTargetValue(float normalizedRotationValue);
    public abstract void Tick();


    protected Vector3 GetRotationAxis()
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
