using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField][Range(0, 360)] int offset;
    [SerializeField] bool clockwise;
    [SerializeField] float period;
    [Space]
    [SerializeField] SignalActivator signal;

    Vector3 standartRotation;

    void Start()
    {
        float angvel = 360f / period;
        if (clockwise)
            angvel *= -1;

        transform.Rotate(new Vector3(0f, 0f, offset));
        standartRotation = new Vector3(0f, 0f, angvel);
    }

    void Update()
    {
        if (signal != null && !signal.activated)
            return;


        transform.Rotate(standartRotation * Time.deltaTime);
    }
}
