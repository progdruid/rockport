using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public GameObject CameraBetweenTransitions;
    public GameObject TransitionPrefab;

    private GameObject attachedCamera;
    private Animator animator;

    public bool transitionMade { get; private set; } = true;

    public void AddAttachedCamera (GameObject camera)
    {
        attachedCamera = camera;
        animator = Instantiate(TransitionPrefab,camera.transform.position+Vector3.forward,Quaternion.identity).GetComponent<Animator>();
        animator.transform.parent = attachedCamera.transform;
    }

    public IEnumerator TransiteIn()
    {
        transitionMade = false;
        animator.SetTrigger("Transition");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Full");
        attachedCamera.SetActive(false);
        CameraBetweenTransitions.SetActive(true);
        transitionMade = true;
    }

    public IEnumerator TransiteOut()
    {
        transitionMade = false;
        CameraBetweenTransitions.SetActive(false);
        attachedCamera.SetActive(true);
        animator.SetTrigger("Transition");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        transitionMade = true;
    }

    private void Update()
    {
        Debug.Log(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }
}
