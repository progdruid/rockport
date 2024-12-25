using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CustomSoundEmitter))]
public class TransitionVeil : MonoBehaviour
{
    [SerializeField] bool closedAtStart;

    public bool closed { get; private set; }
    public bool inTransition { get; private set; }

    private Animator animator;
    private CustomSoundEmitter soundEmitter;

    void Awake()
    {
        if (GameSystems.Ins != null)
            GameSystems.Ins.TransitionVeil = this;

        animator = GetComponent<Animator>();
        soundEmitter = GetComponent<CustomSoundEmitter>();

        closed = closedAtStart;
        animator.SetBool("Closed", closed);
    }

    public IEnumerator TransiteIn()
    {
        if (!closed)
        {
            inTransition = true;
            if (GameSystems.Ins.InputSet != null && GameSystems.Ins.DeathsBar != null)
                GameSystems.Ins.InputSet.Active = false;
            

            yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Start"));
            soundEmitter.EmitSound("TransiteIn");
            animator.SetBool("Closed", true);

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Closed"));

            inTransition = false;
            closed = true;
        }
    }

    public IEnumerator TransiteOut()
    {
        if (closed)
        {
            inTransition = true;
            closed = false;

            yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Start"));
            soundEmitter.EmitSound("TransiteOut");
            animator.SetBool("Closed", false);

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Open"));

            if (GameSystems.Ins.InputSet != null && GameSystems.Ins.DeathsBar != null)
                GameSystems.Ins.InputSet.Active = true;
            
            inTransition = false;
        }
    }
}
