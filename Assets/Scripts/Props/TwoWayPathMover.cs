using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWayPathMover : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    [SerializeField] SignalActivator inverseSignal;
    [Space]
    public float timePeriod;
    public Transform[] entities;
    public Transform[] anchors;

    private float generalDist;
    private float[] dists;
    private float timePassed = 0f;

    private bool invalid => anchors == null || entities == null || (anchors.Length == 0 || entities.Length == 0);

    #region Unity funcs
    private void Start()
    {
        Init();
        Move(0f);
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
        if (invalid)
            return;

        //calculating distances
        dists = new float[anchors.Length];
        generalDist = 0f;
        for (int i = 0; i < anchors.Length; i++)
        {
            dists[i] = Vector2.Distance(anchors[i].position, anchors[(i + 1) % anchors.Length].position);
            if (i < anchors.Length - 1)
                generalDist += dists[i];
        }
    }

    void Update()
    {
        if (signal == null || inverseSignal == null)
            return;

        Move(timePassed);
        
        if (signal.activated && !inverseSignal.activated)
            timePassed += Time.deltaTime;
        else if (!signal.activated && inverseSignal.activated)
            timePassed -= Time.deltaTime;

        if (timePassed < 0f)
            timePassed = 0f;
        else if (timePassed > timePeriod)
            timePassed = timePeriod;
    }

    private void Move(float time)
    {
        if (invalid)
            return;

        for (int i = 0; i < entities.Length; i++)
        {
            float distOffset = generalDist * (time / timePeriod);
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
