using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMover : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    [Space]
    public float timePeriod;
    public MoverMotionType motionType;
    public Transform anchor;
    public float radius;
    public TransformOffsetPair[] entities;
   
    private float timePassed = 0f;
    private bool activated = true;

    private System.Func<float, float> enterpFunc;
    private bool invalid => anchor == null || entities == null || (entities.Length == 0);

    #region interp funcs

    private float LinearInterp(float x) => x;
    private float SineInterp(float x) => Mathf.Clamp(1f - (Mathf.Cos(x * Mathf.PI * 2f) + 1f) / 2f, 0f, 1f);
    private float PongInterp(float x) => Mathf.PingPong(x * 2f, 1f);

    #endregion

    #region Unity funcs
    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        if (signal != null)
            signal.ActivationUpdateEvent -= HandleActivation;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Init();
        Move(0f);
    }
#endif
    #endregion

    private void Init()
    {
        if (signal != null)
        {
            activated = false;
            signal.ActivationUpdateEvent += HandleActivation;
        }

        if (invalid)
            return;

        switch (motionType)
        {
            case MoverMotionType.Linear:
                enterpFunc = LinearInterp;
                break;
            case MoverMotionType.Sine:
                enterpFunc = SineInterp;
                break;
            case MoverMotionType.Pong:
                enterpFunc = PongInterp;
                break;
        }
    }

    void Update()
    {
        if (!activated)
            return;

        if (timePassed > timePeriod)
            timePassed = 0f;

        Move(timePassed);

        timePassed += Time.deltaTime;
    }

    private void Move(float time)
    {
        if (invalid)
            return;

        for (int i = 0; i < entities.Length; i++)
        {
            float angle = enterpFunc(time / timePeriod + entities[i].offset) * Mathf.PI * 2f;
            Vector2 addvector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            entities[i].transform.position = (Vector2)anchor.position + addvector;
        }
    }

    private void HandleActivation(bool active, GameObject source) => activated = active;
}
