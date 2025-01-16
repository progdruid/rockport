using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Map
{

[RequireComponent(typeof(Animator))]
public class JumpPad : PropEntity
{
    private static readonly int Pressed = Animator.StringToHash("Pressed");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float impulse;
    [SerializeField] private float timeOffset;
    [SerializeField] private float cooldown;
    [SerializeField] private Animator animator;

    [SerializeField] private UnityEvent onJump;

    private readonly List<(Collider2D col, bool isPlayer, float time)> _bodiesInside = new();


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;

        var handle = new PropertyHandle()
        {
            PropertyName = "Impulse",
            PropertyType = PropertyType.Decimal,
            Getter = () => impulse,
            Setter = (object input) => impulse = (float)input
        };
        yield return handle;
    }

    public override string Pack() => JsonUtility.ToJson((base.Pack(), impulse));

    public override void Unpack(string data)
    {
        string baseData;
        (baseData, impulse) = JsonUtility.FromJson<(string, float)>(data);
        base.Unpack(baseData);
    }


    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Corpse"))
            return;
        
        var body = (other, other.CompareTag("Player"), Time.time);
        _bodiesInside.Add(body);

        StartCoroutine(Push(body));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _bodiesInside.RemoveAll((x) => x.col == other);

        if (other.CompareTag("Player"))
            GameSystems.Ins.InputSet.CanJump = true;
    }

    private void Update()
    {
        for (var i = 0; i < _bodiesInside.Count; i++)
            if (_bodiesInside[i].time - Time.time >= cooldown)
            {
                _bodiesInside[i] = (_bodiesInside[i].col, _bodiesInside[i].isPlayer, Time.time);
                StartCoroutine(Push(_bodiesInside[i]));
            }
    }



    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator Push((Collider2D col, bool isPlayer, float time) pressingBody)
    {
        onJump.Invoke();
        animator.SetBool(Pressed, true);
        yield return new WaitForSeconds(timeOffset);
        animator.SetBool(Pressed, false);
    }
}

}