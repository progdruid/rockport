using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMover : MonoBehaviour
{
    public float timePeriod;
    public MoverMotionType motionType;
    public Transform anchor;
    public float radius;
    public TransformOffsetPair[] entities;
   
    private float timePassed = 0f;

    private System.Func<float, float> enterpFunc;
    private bool invalid => anchor == null || entities == null || (entities.Length == 0);

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

    private void Init()
    {
        if (invalid)
            return;

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
}
