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
        if (Registry.ins != null)
            Registry.ins.transitionVeil = this;

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
            if (Registry.ins.inputSet != null && Registry.ins.deathsBar != null)
            {
                Registry.ins.inputSet.Active = false;
                Registry.ins.deathsBar.SetActive(false);
            }

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

            if (Registry.ins.inputSet != null && Registry.ins.deathsBar != null)
            {
                Registry.ins.inputSet.Active = true;
                Registry.ins.deathsBar.SetActive(true);
            }
            inTransition = false;
        }
    }
}
