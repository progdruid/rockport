using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoverMotionType
{
    Linear,
    Sine,
    Pong
}

[System.Serializable]
public class TransformOffsetPair
{
    public Transform transform;
    [Range(0f, 1f)]
    public float offset;
}

public class PathMover : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    [Space]
    public float timePeriod;
    public MoverMotionType motionType;
    public TransformOffsetPair[] entities;
    public Transform[] anchors;

    private float generalDist;
    private float[] dists;
    private float timePassed = 0f;
    private System.Func<float, float> interpFunc;

    private bool invalid => anchors == null || entities == null || (anchors.Length == 0 || entities.Length == 0);

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

#if UNITY_EDITOR
    void OnValidate()
    {
        Init();
        Move(0f);
    }
#endif
    #endregion

    private void Init ()
    {
        if (invalid)
            return;

        //calculating distances
        dists = new float[anchors.Length];
        generalDist = 0f;
        for (int i = 0; i < anchors.Length; i++)
        {
            dists[i] = Vector2.Distance(anchors[i].position, anchors[(i + 1)%anchors.Length].position);
            generalDist += dists[i];
        }

        switch (motionType)
        {
            case MoverMotionType.Linear:
                interpFunc = LinearInterp;
                break;
            case MoverMotionType.Sine:
                interpFunc = SineInterp;
                break;
            case MoverMotionType.Pong:
                interpFunc = PongInterp;
                break;
        }
    }

    void Update()
    {
        if (signal != null && !signal.activated)
            return;

        if (timePassed > timePeriod)
            timePassed = 0f;

        Move(timePassed);

        timePassed += Time.deltaTime;
    }

    private void Move (float time)
    {
        if (invalid)
            return;

        for (int i = 0; i < entities.Length; i++)
        {
            float distOffset = generalDist * (interpFunc(entities[i].offset + (time / timePeriod)) % 1f);
            float tempDist = 0f;
            int anchorIndex = -1;

            while (tempDist <= distOffset)
            {
                anchorIndex++;
                tempDist += dists[anchorIndex];
            }

            float t = 1f - (tempDist - distOffset) / dists[anchorIndex];
            entities[i].transform.position = Vector2.Lerp(anchors[anchorIndex].position, anchors[(anchorIndex + 1) % anchors.Length].position, t);
        }
    }
}
