using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public GameObject cameraBetweenTransitions;

    private DeathsBarManager deathsbar;

    private void Start()
    {
        Registry.ins.transitionController = this;

        deathsbar = GetComponent<DeathsBarManager>();
    }

    public IEnumerator TransiteIn()
    {
        InputSystem.ins.SetActive(false);
        deathsbar.SetActive(false);

        Animator animator = Camera.main.transform.GetChild(0).GetComponent<Animator>();
        animator.SetTrigger("Transition");

        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Full");

        Camera.main.gameObject.SetActive(false);
        cameraBetweenTransitions.SetActive(true);
    }

    public IEnumerator TransiteOut()
    {
        cameraBetweenTransitions.SetActive(false);
        Camera.main.gameObject.SetActive(true);

        Animator animator = Camera.main.transform.GetChild(0).GetComponent<Animator>();
        animator.SetTrigger("Transition");

        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");

        InputSystem.ins.SetActive(true);
        deathsbar.SetActive(true);
    }
}
