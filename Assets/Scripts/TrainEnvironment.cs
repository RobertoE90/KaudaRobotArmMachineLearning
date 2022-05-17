using System;
using System.Collections;
using UnityEngine;

public class TrainEnvironment : MonoBehaviour
{
    [SerializeField] private KaudaArmController _kaudaArmController;
    [SerializeField] private Transform _ikTargetObject;
    
    private Color _feedbackColor;
    private bool _lastEpisodeSucceed = true;
    
    public const  float MIN_DISTANCE = 0.475f;
    public const float MAX_DISTANCE = 0.725f;

    private int _repeatCounter = -1;

    private void Awake()
    {
        _kaudaArmController.OnEpisodeBeginAction += ConfigureScene;
        _kaudaArmController.EpisodeEnded += EpisodeEndedFeedback;
        _feedbackColor = Color.gray;
    }

    private void ConfigureScene()
    {
        if(!_lastEpisodeSucceed)
        {
            _repeatCounter--;
            if (_repeatCounter > 0)
                return;
        }

        _repeatCounter = -1;

        var distance = UnityEngine.Random.Range(MIN_DISTANCE, MAX_DISTANCE);
        var rotation = Quaternion.Euler(0, UnityEngine.Random.value * 360, 0) * Quaternion.Euler(UnityEngine.Random.Range(-10, -90), 0, 0);

        _ikTargetObject.localPosition = rotation * Vector3.forward * distance;
        _ikTargetObject.localRotation = rotation * Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 180)) * Quaternion.Euler(0, UnityEngine.Random.Range(-90, 90), 0);
    }

    private void EpisodeEndedFeedback(bool succeed, int repeatCount)
    {
        _lastEpisodeSucceed = succeed;
        if (!succeed && _repeatCounter == -1)
        {
            _repeatCounter = repeatCount;
        }


        _feedbackColor = succeed ? Color.green : Color.red;
        StartCoroutine(RestoreFeedbackCoroutine());
    }

    private IEnumerator RestoreFeedbackCoroutine()
    {
        for (var i = 0; i < 10; i++)
            yield return null;
        _feedbackColor = Color.gray;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0, 1));

        Gizmos.color = _feedbackColor;
        if (_feedbackColor == null)
            Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(Vector3.zero, MIN_DISTANCE);
        
        Gizmos.DrawWireSphere(Vector3.zero, MAX_DISTANCE);

        Gizmos.matrix = _ikTargetObject.localToWorldMatrix;
        Gizmos.color = _kaudaArmController.CurrentInRange ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(Vector3.zero, _kaudaArmController.InRangeDistanceValue);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(Vector3.forward * 0.05f, new Vector3(0.005f, 0.005f, 0.1f));
        Gizmos.color = Color.green;
        Gizmos.DrawCube(Vector3.up * 0.015f, new Vector3(0.005f, 0.03f, 0.005f));
        Gizmos.color = Color.red;
        Gizmos.DrawCube(Vector3.right * 0.015f, new Vector3(0.03f, 0.005f, 0.005f));
    }


    private void OnGUI()
    {
        if (!Application.isPlaying || !_kaudaArmController.DebugActive)
            return;

        //observation
        GUILayout.BeginArea(new Rect(50, Screen.height - 250, Screen.width, Screen.height));
        GUILayout.Label("-env--");
        GUILayout.Label($"RepeatCounter {_repeatCounter}");
        GUILayout.EndArea();
    }
}
