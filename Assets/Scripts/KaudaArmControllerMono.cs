using UnityEngine;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class KaudaArmControllerMono : MonoBehaviour
{
    [SerializeField] private Transform _armEndTransform;
    [SerializeField] private Transform[] _armJoints;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _openCloseHand;
    public float OpenCloseRange
    {
        set
        {
            var clampedValue = Mathf.Clamp01(value);
            _openCloseHand = clampedValue;
        }
        get {
            return _openCloseHand;
        }
    }
    [SerializeField] private MotorsControllers[] _armMotorsControllers;
    [Space(20)]
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        for (var i = 0; i < _armMotorsControllers.Length; i++)
        {
            _armMotorsControllers[i].NormalizedRotationValue = 0.5f;
            _armMotorsControllers[i].Motor.InitRotationAt(_armMotorsControllers[i].NormalizedRotationValue);
        }
    }

    private void Update()
    {
        Tick();
    }

    private void Tick()
    {
        for(var i = 0; i < _armMotorsControllers.Length; i++)
        {
            _armMotorsControllers[i].ApplyValueToMotor();
            _armMotorsControllers[i].Motor.Tick();
        }
        _animator.SetFloat("OpenCloseFinguers", _openCloseHand);

        
    }


    [Serializable]
    public struct MotorsControllers
    {
        [Range(0, 1)] public float NormalizedRotationValue;
        public NemaMotorControllerBase Motor;

        public void ApplyValueToMotor()
        {
            Motor.SetTargetValue(NormalizedRotationValue);
        }
    }
}
