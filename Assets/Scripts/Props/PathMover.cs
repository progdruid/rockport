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
    [SerializeField] SignalSource signal;
    [Space]
    public float timePeriod;
    public TransformOffsetPair[] entities;
    public Transform[] anchors;

    private float generalDist;
    private float[] dists;
    private float timePassed = 0f;

    private bool invalid => anchors == null || entities == null || (anchors.Length == 0 || entities.Length == 0);

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
    }

    void Update()
    {
        if (signal != null && !signal.activated)
            return;

        timePassed = ((timePassed + Time.deltaTime) % timePeriod + timePeriod) % timePeriod;

        Move(timePassed);

    }

    private void Move (float time)
    {
        if (invalid)
            return;

        for (int i = 0; i < entities.Length; i++)
        {
            float distOffset = generalDist * ((entities[i].offset + time / timePeriod) % 1f);
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
