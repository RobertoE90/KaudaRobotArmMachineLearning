using UnityEngine;

public abstract class NemaMotorControllerBase : MonoBehaviour
{
    public abstract void SetTargetValue(float normalizedRotationValue);
    public abstract void Tick();
}
