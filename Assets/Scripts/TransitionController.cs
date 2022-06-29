using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public GameObject CameraBetweenTransitions;
    public GameObject TransitionPrefab;

    private GameObject attachedCamera;
    private Animator animator;

    public void AddAttachedCamera (GameObject camera)
    {
        attachedCamera = camera;
        animator = Instantiate(TransitionPrefab,camera.transform.position+Vector3.forward,Quaternion.identity).GetComponent<Animator>();
        animator.transform.parent = attachedCamera.transform;
    }

    public IEnumerator TransiteIn()
    {
        animator.SetTrigger("Transition");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Full");
        attachedCamera.SetActive(false);
        CameraBetweenTransitions.SetActive(true);
    }

    public IEnumerator TransiteOut()
    {
        CameraBetweenTransitions.SetActive(false);
        attachedCamera.SetActive(true);
        animator.SetTrigger("Transition");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
    }
}
