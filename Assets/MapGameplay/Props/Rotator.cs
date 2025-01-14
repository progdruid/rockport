using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField][Range(0, 360)] int offset;
    [SerializeField] bool clockwise;
    [SerializeField] float period;
    [Space]
    [SerializeField] SignalSource signal;

    float timeOffset = 0f;
    int clockwiseMult = 1;

    void Start()
    {
        if (clockwise)
            clockwiseMult *= -1;

        timeOffset = offset / 360f * period;
    }

    void Update()
    {
        if (signal != null && !signal.Activated)
            return;

        transform.rotation = Quaternion.Euler(0f, 0f, clockwiseMult * (Time.time - timeOffset) / period * 360f);
    }
}
