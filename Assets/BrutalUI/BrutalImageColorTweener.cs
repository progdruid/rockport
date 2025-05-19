using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BrutalUI
{

[RequireComponent(typeof(Image))]
public class BrutalImageColorTweener  : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Color targetColor;
    
    private Image _image;
    
    private Color _startColor;
    private Color _endColor;

    private float _t;
    private float _targetT;
    private Coroutine _tweenRoutine;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _image = GetComponent<Image>();
        _startColor = _image.color;
        _endColor = targetColor;
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    [ContextMenu("Move To Start")]
    public void TweenToStart() => Tween(0f);
    [ContextMenu("Move To Target")]
    public void TweenToTarget() => Tween(1f);
    
    [ContextMenu("Snap To Start")]
    public void SnapToStart() => Snap(0f);
    [ContextMenu("Snap To Target")]
    public void SnapToTarget() => Snap(1f);

    public bool IsAtStart() => _t.IsApproximately(0f);
    public bool IsAtTarget() => _t.IsApproximately(1f);
    public bool IsFinished() => _t.IsApproximately(_targetT);
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Snap(float target)
    {
        _t = _targetT = target;
        _image.color = Color.Lerp(_startColor, _endColor, target);
        if (_tweenRoutine != null)
            StopCoroutine(_tweenRoutine);
        _tweenRoutine = null;
    }

    private void Tween(float target)
    {
        _targetT = target;
        _tweenRoutine ??= StartCoroutine(MoveRoutine());
    }
    
    private IEnumerator MoveRoutine()
    {
        while (!_t.IsApproximately(_targetT))
        {
            var deltaT = Time.deltaTime / duration;
            _t = _t.MoveTo(_targetT, deltaT);
            var t = _t * _t * (3f - 2f * _t);
            _image.color = Color.Lerp(_startColor, _endColor, t);
            yield return null;
        }

        _tweenRoutine = null;
    }
}

}