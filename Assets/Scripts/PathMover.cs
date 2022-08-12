using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoverMotionType
{
    Linear,
    Sine,
    Pong
}

public class PathMover : MonoBehaviour
{
    public Transform[] entities;
    [Range(0f, 1f)]
    public float[] offsets;
    public float timePeriod;
    public Transform[] anchors;
    public MoverMotionType motionType;

    private float generalDist;
    private float[] dists;

    private float timePassed = 0f;

    private System.Func<float, float> enterpFunc;

    #region interp funcs

    private float LinearEnterp(float x) => x;
    private float SineEnterp(float x) => Mathf.Clamp(1f - (Mathf.Cos(x * Mathf.PI * 2f) + 1f) / 2f, 0f, 1f);
    private float PongEnterp(float x) => Mathf.PingPong(x * 2f, 1f);

    #endregion

    #region Unity funcs
    private void Start()
    {
        Init();
    }

    void OnValidate()
    {
        Init();
        Move(0f);
    }
    #endregion

    private void Init ()
    {
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
                enterpFunc = LinearEnterp;
                break;
            case MoverMotionType.Sine:
                enterpFunc = SineEnterp;
                break;
            case MoverMotionType.Pong:
                enterpFunc = PongEnterp;
                break;
        }
    }

    void Update()
    {
        if (timePassed > timePeriod)
            timePassed = 0f;

        Move(timePassed);

        timePassed += Time.deltaTime;
    }

    private void Move (float time)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            float distOffset = generalDist * (enterpFunc(offsets[i] + (time / timePeriod)) % 1f);
            float tempDist = 0f;
            int anchorIndex = -1;

            while (tempDist <= distOffset)
            {
                anchorIndex++;
                tempDist += dists[anchorIndex];
            }

            float t = 1f - (tempDist - distOffset) / dists[anchorIndex];
            entities[i].position = Vector2.Lerp(anchors[anchorIndex].position, anchors[(anchorIndex + 1) % anchors.Length].position, t);
        }
    }
}
