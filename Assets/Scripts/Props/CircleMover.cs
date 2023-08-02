using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMover : MonoBehaviour
{
    [SerializeField] SignalSource signal;
    [Space]
    public float timePeriod;
    public MoverMotionType motionType;
    public bool flip;
    public Transform anchor;
    public float radius;
    public TransformOffsetPair[] entities;
   
    private float timePassed = 0f;

    private bool invalid => anchor == null || entities == null || (entities.Length == 0);

#if UNITY_EDITOR
    void OnValidate()
    {
        Move(0f);
    }
#endif

    void Update()
    {
        if (signal != null && !signal.activated)
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
            float interpVal = ((time / timePeriod + entities[i].offset) % 1f);
            if (flip)
                interpVal = 1f - interpVal;
            float angle = interpVal * Mathf.PI * 2f;
            Vector2 addvector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            entities[i].transform.position = (Vector2)anchor.position + addvector;
        }
    }
}
