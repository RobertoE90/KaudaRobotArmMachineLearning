using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ManipulatorColorChangeTool : MonoBehaviour
{
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _movingColor;
    [SerializeField] private Color _hoverColor;
    [Space(20)]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator _objectManipulator;

    private void Awake()
    {
        SetDefaultColor();

        _objectManipulator.OnManipulationEnded.AddListener(delegate { ChangeMaterialColorTo(_hoverColor); });
        _objectManipulator.OnManipulationStarted.AddListener(delegate { ChangeMaterialColorTo(_movingColor); });

        _objectManipulator.OnHoverEntered.AddListener(delegate { ChangeMaterialColorTo(_hoverColor); });
        _objectManipulator.OnHoverExited.AddListener(delegate { ChangeMaterialColorTo(_defaultColor); });
    }

    public void SetDefaultColor()
    {
        ChangeMaterialColorTo(_defaultColor);
    }

    public void ChangeMaterialColorTo(Color color)
    {
        _renderer.material.color = color;
    }
}
