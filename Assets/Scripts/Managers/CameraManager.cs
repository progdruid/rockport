using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(Rigidbody2D))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] float closeSpeed;
    [SerializeField] float minimalRange;

    private new Camera camera;
    private Rigidbody2D rb;
    private Animator transition;
    private Transform target;

    private bool transiting = true;

    private void OnEnable() => Registry.ins.cameraManager = this;
    void Start()
    {
        camera = GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        transition = camera.transform.GetChild(0).GetComponent<Animator>();

        Registry.ins.playerManager.PlayerSpawnEvent += HandlePlayerSpawn;
    }

    private void OnDestroy()
    {
        Registry.ins.playerManager.PlayerSpawnEvent -= HandlePlayerSpawn;
    }

    void Update()
    {
        if (transiting)
            return;
    }

    public void SetTarget (Transform target)
    {
        this.target = target;
        transform.parent = target;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    public void Untarget()
    {
        transform.parent = null;
        target = null;
    }

    public IEnumerator TransiteIn ()
    {
        Registry.ins.inputSystem.SetActive(false);
        Registry.ins.deathsBar.SetActive(false);

        transition.SetTrigger("Transition");
        transiting = true;

        yield return new WaitUntil(() => transition.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Full");
    }

    public IEnumerator TransiteOut ()
    {
        transition.SetTrigger("Transition");

        yield return new WaitUntil(() => transition.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        
        Registry.ins.inputSystem.SetActive(true);
        Registry.ins.deathsBar.SetActive(true);
        transiting = false;
    }

    private void HandlePlayerSpawn(GameObject player) => SetTarget(player.transform);
}
