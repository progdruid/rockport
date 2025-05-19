using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class BrutalABTweener : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private RectTransform targetPosition;
    
    private RectTransform _rect;
    
    private Vector2 _startPosition;
    private Vector2 _endPosition;

    private float _t;
    private float _targetT;
    private Coroutine _moveRoutine;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(targetPosition);
        _startPosition = _rect.anchoredPosition;
        var targetLocalPos = _rect.InverseTransformPoint(targetPosition.position);
        _endPosition = _startPosition + new Vector2(
            targetLocalPos.x,// + _rect.pivot.x * _rect.rect.width,
            targetLocalPos.y// + _rect.pivot.y * _rect.rect.height
        );
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    [ContextMenu("Tween To Start")]
    public void TweenToStart() => Tween(0f);
    [ContextMenu("Tween To Target")]
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
        _rect.anchoredPosition = Vector2.Lerp(_startPosition, _endPosition, target);
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);
        _moveRoutine = null;
    }

    private void Tween(float target)
    {
        _targetT = target;
        _moveRoutine ??= StartCoroutine(TweenRoutine());
    }
    
    private IEnumerator TweenRoutine()
    {
        while (!_t.IsApproximately(_targetT))
        {
            var deltaT = Time.deltaTime / duration;
            _t = _t.MoveTo(_targetT, deltaT);
            var t = _t * _t * (3f - 2f * _t);
            _rect.anchoredPosition = Vector2.Lerp(_startPosition, _endPosition, t);
            yield return null;
        }

        _moveRoutine = null;
    }
}

}