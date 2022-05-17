using UnityEngine;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class KaudaArmController : Agent
{
    [SerializeField][Range(-1, 1)] private int[] _actionSlider;
    [Space(20)]
    [SerializeField] private bool _isTraining;
    [SerializeField] private Transform _targetIKPoint;
    [SerializeField] private Transform _armEndTransform;
    [SerializeField] private Transform _forearmTransform;

    [SerializeField] private Lesson _defaultInRange;
    [Space(20)]
    [SerializeField] private Lesson[] _lessonsList;
    private int _lessonIndex;
    
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

    [SerializeField] private bool _debugObservations;
    public bool DebugActive => _debugObservations;

    private bool _resetArm;

    private bool _currentInRange = false;
    private bool _currentInDistanceRange = false;
    private bool _entererInRangeOnTheEpisode = false;
    
    public event Action OnEpisodeBeginAction;
    public event Action<bool, int> EpisodeEnded; //true for success, and amount of repeats

    private float[] _rewardsViewer;
    
    //discrete heuristics vars
    private int _currentControlledMotorIndex = 0;

    
    public float InRangeDistanceValue {
        get {
            var lesson = (_lessonIndex == -1) ? _defaultInRange : _lessonsList[_lessonIndex];
            return lesson.CenterDistanceThreshold; 
        }
        
    }
    public bool CurrentInRange => _currentInRange;

    public override void Initialize()
    {
        base.Initialize();

        _lessonIndex = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lesson_index_value", -1);
        MaxStep = _isTraining ? 700 : 0;
        
        _rewardsViewer = new float[3];
    }

    public override void OnEpisodeBegin()
    {
        _lessonIndex = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lesson_index_value", -1);
        _entererInRangeOnTheEpisode = false;
        _currentInRange = false;
        if (_resetArm) {
            for (var i = 0; i < _armMotorsControllers.Length; i++)
            {

                _armMotorsControllers[i].ActivateMotor = true;
                _armMotorsControllers[i].NormalizedRotationValue = 0.5f;
                _armMotorsControllers[i].Motor.InitRotationAt(_armMotorsControllers[i].NormalizedRotationValue);
            }
        }

        OnEpisodeBeginAction?.Invoke();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_targetIKPoint == null)
            return;

        sensor.AddObservation(_targetIKPoint.localRotation);//4
        sensor.AddObservation(_targetIKPoint.localPosition);//+3=7

        sensor.AddObservation((_armEndTransform.position - _targetIKPoint.position).magnitude); //+1=8
        var targetOrientationDot = Vector3.Dot(_armEndTransform.rotation * Vector3.up, _targetIKPoint.rotation * Vector3.forward);
        sensor.AddObservation(targetOrientationDot); //+1=9

        var forearmOrientationDot = Vector3.Dot(_forearmTransform.rotation * Vector3.up, _targetIKPoint.rotation * Vector3.forward);
        sensor.AddObservation(forearmOrientationDot); //+1=10

        var inRangeObservation = 0f;
        if (_currentInDistanceRange)
            inRangeObservation = 1;
        if (_currentInRange)
            inRangeObservation = 2;
        
        sensor.AddObservation(inRangeObservation); //+1=11
        
        foreach (var motorController in _armMotorsControllers) //+5=17
        {
            var rotAngle = motorController.Motor.GetRotationDotValue();
            sensor.AddObservation(rotAngle);
        }
        
    }

    public override void Heuristic(float[] actionsOut)
    {
        if(actionsOut.Length != _actionSlider.Length)
        {
            Debug.LogError($"Slider does not match with motors {actionsOut.Length} != {_actionSlider.Length}");
            return;
        }

        for(var i = 0; i < _actionSlider.Length; i++)
        {
            actionsOut[i] = _actionSlider[i] + 1;
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {

        if (_targetIKPoint == null)
            return;

        _lessonIndex = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lesson_index_value", -1);

        
        //discrete actions
        for (var i = 0; i < _armMotorsControllers.Length; i++)
        {
            _armMotorsControllers[i].Motor.DiscreteControl((int)vectorAction[i] - 1);
        }

        Tick();
        CollectRewards();
    }

    private void CollectRewards()
    {
        var distanceToTarget = (_armEndTransform.position - _targetIKPoint.position).magnitude;
        var orientationDot = Vector3.Dot(_armEndTransform.rotation * Vector3.up, _targetIKPoint.rotation * Vector3.forward);
        var lesson = (_lessonIndex == -1) ? _defaultInRange : _lessonsList[_lessonIndex];
        _currentInDistanceRange = distanceToTarget <= lesson.CenterDistanceThreshold;
        
        var inOrientationRange = (1f - orientationDot) <= lesson.OrientationThreshold;

        _currentInRange = _currentInDistanceRange && inOrientationRange;

        if (!_isTraining)
            return;

        if (_currentInRange)
            _entererInRangeOnTheEpisode = true;

        var armEndLocal = transform.InverseTransformPoint(_armEndTransform.position);
        if(armEndLocal.y < -0.15f)
        {
            AddReward(-3f); //don't kill yourself
            EndEpisode(true, false);
        }

        
        _rewardsViewer[0] = orientationDot * 0.5f;
        AddReward(_rewardsViewer[0] / MaxStep);

        var distanceReward = 1f - Mathf.Pow((distanceToTarget / (TrainEnvironment.MAX_DISTANCE * 0.5f)), 0.5f);
        _rewardsViewer[1] = distanceReward;
        AddReward(_rewardsViewer[1] / MaxStep);

        var inRangeReward = (_currentInRange ? 1f : -2f);
        _rewardsViewer[2] = inRangeReward;
        AddReward(_rewardsViewer[2] / MaxStep);

        
        Academy.Instance.StatsRecorder.Add("MyStats/Orientation reward", _rewardsViewer[0]);
        Academy.Instance.StatsRecorder.Add("MyStats/Distance reward", _rewardsViewer[1]);
        Academy.Instance.StatsRecorder.Add("MyStats/In range reward", _rewardsViewer[2]);
        
        
        if (_isTraining && StepCount >= MaxStep - 1)
        {
            EndEpisode(!_entererInRangeOnTheEpisode, _entererInRangeOnTheEpisode, lesson.RepeatFailCounter);
        }
    }


    private void EndEpisode(bool reset, bool succed, int repeatCount = -1)
    {
        _resetArm = reset;
        EpisodeEnded?.Invoke(succed, repeatCount);
        EndEpisode();
    }

    private void Tick()
    {
        for (var i = 0; i < _armMotorsControllers.Length; i++)
        {
            _armMotorsControllers[i].Motor.Tick();
        }
        _animator.SetFloat("OpenCloseFinguers", _openCloseHand);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = _armMotorsControllers[_currentControlledMotorIndex].Motor.transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.25f);
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !_debugObservations)
            return;

        //observation
        GUILayout.BeginArea(new Rect(50, 30, 200, 1000));
        GUILayout.Label("-----Observation ----");
        var observations = GetObservations();
        for (var i = 0; i < observations.Count; i++)
        {
            var obser = observations[i];
            if(i == 12)
                GUILayout.Label("---------");
            GUILayout.Label($"{i} obs:{obser}");
        }
        GUILayout.EndArea();
        
        //actions
        GUILayout.BeginArea(new Rect(250, 30, 200, 1000));
        GUILayout.Label("----- Actions area ----");
        var actions = GetAction();
        foreach (var action in actions)
        {
            GUILayout.HorizontalSlider(action - 1f, -1f, 1f);
        }
        GUILayout.EndArea();

        //reward
        GUILayout.BeginArea(new Rect(500, 30, 200, 1000));
        GUILayout.Label("-----reward area ----");
        foreach(var r in _rewardsViewer)
        {
            GUILayout.Label($"r: {r}");
        }
        GUILayout.Label($"--------Reward Comulative------: {GetCumulativeReward()}");
        GUILayout.EndArea();

        //time
        GUILayout.BeginArea(new Rect(750, 30, 200, 1000));
        GUILayout.Label("-----time ----");
        GUILayout.Label($"Time {StepCount} / {MaxStep}");
        GUILayout.EndArea();
    }

    [Serializable]
    public struct MotorsControllers
    {
        public bool ActivateMotor;
        [Range(0, 1)] public float NormalizedRotationValue;
        public NemaMotorControllerBase Motor;

        public void ApplyValueToMotor()
        {
            if(ActivateMotor)
                Motor.SetTargetValue(NormalizedRotationValue);
        }
    }
    
    
    [Serializable]
    public struct Lesson
    {
        public float CenterDistanceThreshold;
        public float OrientationThreshold;
        public int RepeatFailCounter;
    }
}

