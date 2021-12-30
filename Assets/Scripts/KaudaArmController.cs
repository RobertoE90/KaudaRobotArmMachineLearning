using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KaudaArmController : MonoBehaviour
{
    [SerializeField][Range(0, 1)] private float _openCloseHand;
    [SerializeField] private MotorsControllers[] _armMotorsControllers;
    [Space(20)]
    [SerializeField] private Animator _animator;

    private void Update()
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
        public NemaMotorController Motor;

        public void ApplyValueToMotor()
        {
            Motor.SetTargetValue(NormalizedRotationValue);
        }
    }
}
