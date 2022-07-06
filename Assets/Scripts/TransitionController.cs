using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public GameObject CameraBetweenTransitions;
    public GameObject TransitionPrefab;

    private GameObject attachedCamera;
    private Animator animator;

    private DeathsBarManager deathsbar;
    private Player playerBehaviour;

    private void Start()
    {
        deathsbar = GetComponent<DeathsBarManager>();
    }

    public void SetPlayer (Player _player)
    {
        this.playerBehaviour = _player;
        attachedCamera = _player.transform.GetChild(0).gameObject;
        animator = Instantiate(TransitionPrefab,_player.transform.position-Vector3.forward,Quaternion.identity).GetComponent<Animator>();
        animator.transform.parent = attachedCamera.transform;
    }

    public IEnumerator TransiteIn()
    {
        playerBehaviour.active = false;
        deathsbar.SetActive(false);
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
        playerBehaviour.active = true;
        deathsbar.SetActive(true);
    }
}
